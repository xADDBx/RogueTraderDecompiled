using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Combat;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[TypeId("e8e5532868cba7f49a810fbd4d383a13")]
public class AbilityCustomStarshipRam : AbilityCustomLogic, ICustomShipPathProvider, IAbilityTargetRestriction
{
	private delegate void ProcessNodeDelegate(CustomGridNodeBase currentNode);

	public class RamDamagePrediction
	{
		public DamagePredictionData selfDamage;

		public DamagePredictionData targetDamage;

		public ShieldDamageData selfShields;

		public ShieldDamageData targetShields;

		public StarshipEntity ramTarget;

		public RamDamagePrediction()
		{
			selfDamage = new DamagePredictionData();
			targetDamage = new DamagePredictionData();
			selfShields = new ShieldDamageData();
			targetShields = new ShieldDamageData();
		}
	}

	[SerializeField]
	private int minDistance;

	[SerializeField]
	private int bonusDistanceOnAttackAttempt;

	[SerializeField]
	[Range(0f, 1f)]
	private float visualCellPenetration;

	[SerializeField]
	[Range(0f, 1f)]
	private float onHitActionsCellPenetration = 1f;

	[SerializeField]
	private float fallBackTime;

	[SerializeField]
	private GameObject PassThroughMarker;

	[SerializeField]
	private GameObject FinalNodeMarker;

	[SerializeField]
	private BlueprintFeatureReference m_DamageBonusTalent;

	[SerializeField]
	private BlueprintFeatureReference m_FirestartingFeature;

	[SerializeField]
	[Tooltip("При отсутствии указанной цели, искать любую, используется в АИ абилках")]
	private bool AllowAutotarget;

	[SerializeField]
	private ActionList ActionsOnHitCaster;

	[SerializeField]
	private ActionList RepeatedBySizeActionsOnTarget;

	public int MinDistance => minDistance;

	public int BonusDistanceOnAttackAttempt(StarshipEntity owner)
	{
		int num = (from b in owner.Facts.GetComponents<StarshipRamModifiers>()
			select b.RamDistanceBonus).DefaultIfEmpty().Sum();
		return bonusDistanceOnAttackAttempt + num;
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		StarshipEntity starship = context.Caster as StarshipEntity;
		if (starship == null)
		{
			yield break;
		}
		PartUnitCombatState combatState = starship.CombatState;
		if (combatState == null)
		{
			yield break;
		}
		List<GraphNode> list = new List<GraphNode>();
		List<Vector3> list2 = new List<Vector3>();
		(Dictionary<GraphNode, CustomPathNode>, Dictionary<StarshipEntity, int>) pathData = GetPathData(starship, starship.Position, starship.Forward);
		Dictionary<GraphNode, CustomPathNode> grapNodes = pathData.Item1;
		Dictionary<StarshipEntity, int> item = pathData.Item2;
		CustomGridNodeBase targetNode = (CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node;
		if (!targetNode.TryGetUnit(out var targetUnit) || !item.TryGetValue(targetUnit as StarshipEntity, out var pathLen))
		{
			if (!AllowAutotarget || !item.Any())
			{
				yield break;
			}
			KeyValuePair<StarshipEntity, int> keyValuePair = item.FirstOrDefault();
			targetUnit = keyValuePair.Key;
			pathLen = keyValuePair.Value;
		}
		if (targetUnit != null && !grapNodes.ContainsKey(targetNode))
		{
			bool flag = true;
			foreach (GraphNode key in grapNodes.Keys)
			{
				if (((CustomGridNodeBase)key).TryGetUnit(out var unit) && unit == targetUnit)
				{
					targetNode = (CustomGridNodeBase)key;
					flag = false;
					break;
				}
			}
			if (flag)
			{
				yield break;
			}
		}
		for (CustomPathNode customPathNode = grapNodes[targetNode]; customPathNode != null; customPathNode = customPathNode.Parent)
		{
			GraphNode node = customPathNode.Node;
			list.Insert(0, node);
			list2.Insert(0, node.Vector3Position);
		}
		int count = list2.Count;
		ForcedPath path = ForcedPath.Construct(list2.Take(count), list.Take(count));
		starship.View.StopMoving();
		UnitMovementAgentBase movementAgent = starship.MaybeMovementAgent;
		movementAgent.MaxSpeedOverride = 2f * 30.Feet().Meters / 3f;
		movementAgent.IsCharging = true;
		bool startFromOddDiagonal = combatState.LastDiagonalCount % 2 == 1;
		int straightMoveLength = combatState.LastStraightMoveLength + path.LengthInCells(startFromOddDiagonal);
		int diagonalsCount = combatState.LastDiagonalCount + path.DiagonalsCount();
		UnitMoveToProperParams cmd = new UnitMoveToProperParams(path, straightMoveLength, diagonalsCount, pathLen);
		UnitCommandHandle moveCmdHandle = starship.Commands.AddToQueueFirst(cmd);
		bool onHitLaunched = false;
		while (!moveCmdHandle.IsFinished)
		{
			float magnitude = (targetNode.Vector3Position - starship.Position).magnitude;
			if (magnitude <= (1f - onHitActionsCellPenetration) * GraphParamsMechanicsCache.GridCellSize)
			{
				MaybeRunActionsOnHit();
			}
			if (magnitude <= (1f - visualCellPenetration) * GraphParamsMechanicsCache.GridCellSize)
			{
				starship.View.StopMoving();
			}
			yield return null;
		}
		MaybeRunActionsOnHit();
		RamDamage(starship, targetUnit as StarshipEntity, pathLen - minDistance, 1f);
		StartFires(starship, context, targetUnit as StarshipEntity);
		starship.Navigation.SpeedMode = PartStarshipNavigation.SpeedModeType.LowSpeed;
		movementAgent.MaxSpeedOverride = null;
		movementAgent.IsCharging = false;
		foreach (AbilityData item2 in (from slot in starship.Hull.WeaponSlots
			where slot.Type == WeaponSlotType.Prow && slot.ActiveAbility != null
			select slot.ActiveAbility.Data).ToList())
		{
			ItemEntity sourceItem = item2.SourceItem;
			if (sourceItem != null)
			{
				sourceItem.Charges = 0;
			}
		}
		Vector3 startPosition = starship.Position;
		Vector3 endPosition = grapNodes[targetNode].Parent.Node.Vector3Position;
		float fallBackTimeWithSpeedUp = fallBackTime / Game.CombatAnimSpeedUp;
		double startTime = Game.Instance.TimeController.GameTime.TotalMilliseconds;
		while (true)
		{
			float num = (float)(Game.Instance.TimeController.GameTime.TotalMilliseconds - startTime) / 1000f;
			if (num >= fallBackTimeWithSpeedUp)
			{
				break;
			}
			float num2 = num / fallBackTimeWithSpeedUp;
			starship.Position = Vector3.Lerp(startPosition, endPosition, Mathf.Sin(MathF.PI / 180f * num2 * 90f));
			yield return null;
		}
		starship.Position = endPosition;
		starship.View.AgentASP.Blocker.BlockAtCurrentPosition();
		BaseUnitEntity baseUnitEntity = targetUnit;
		yield return new AbilityDeliveryTarget((baseUnitEntity != null) ? ((TargetWrapper)baseUnitEntity) : target);
		void MaybeRunActionsOnHit()
		{
			if (!onHitLaunched)
			{
				using (context.GetDataScope(starship.ToITargetWrapper()))
				{
					ActionsOnHitCaster?.Run();
				}
				onHitLaunched = true;
			}
		}
	}

	public (Dictionary<GraphNode, CustomPathNode> grapNodes, Dictionary<StarshipEntity, int> targetUnits) GetPathData(StarshipEntity starship, Vector3 casterPosition, Vector3 casterDirection)
	{
		Dictionary<GraphNode, CustomPathNode> dictionary = new Dictionary<GraphNode, CustomPathNode>();
		Dictionary<StarshipEntity, int> dictionary2 = new Dictionary<StarshipEntity, int>();
		float? num = starship.CombatState?.ActionPointsBlue;
		if (num.HasValue)
		{
			float valueOrDefault = num.GetValueOrDefault();
			float bluePointsCost = UnitPredictionManager.Instance.BluePointsCost;
			CustomPathNode parent = null;
			CustomGridNodeBase customGridNodeBase = (CustomGridNodeBase)AstarPath.active.GetNearest(casterPosition).node;
			int num2 = CustomGraphHelper.GuessDirection(casterDirection);
			int num3 = starship.CombatState.LastDiagonalCount;
			int num4 = 0;
			while (true)
			{
				num3 += ((num2 > 3) ? 1 : 0);
				int num5 = ((num3 % 2 != 0 || num2 <= 3) ? 1 : 2);
				num4 += num5;
				if ((float)num4 > valueOrDefault - bluePointsCost + (float)BonusDistanceOnAttackAttempt(starship))
				{
					break;
				}
				customGridNodeBase = customGridNodeBase.GetNeighbourAlongDirection(num2);
				BaseUnitEntity unit;
				bool flag = customGridNodeBase.TryGetUnit(out unit) && unit != starship && !((unit as StarshipEntity)?.IsSoftUnit ?? false);
				CustomPathNode customPathNode = new CustomPathNode
				{
					Node = customGridNodeBase,
					Direction = num2,
					Parent = parent,
					Marker = (flag ? FinalNodeMarker : PassThroughMarker)
				};
				dictionary.Add(customGridNodeBase, customPathNode);
				parent = customPathNode;
				if (!flag)
				{
					continue;
				}
				if (num4 >= minDistance)
				{
					StarshipEntity obj = unit as StarshipEntity;
					if (obj != null && obj.IsEnemy(starship))
					{
						dictionary2.Add(unit as StarshipEntity, num4);
					}
				}
				break;
			}
			return (grapNodes: dictionary, targetUnits: dictionary2);
		}
		return (grapNodes: dictionary, targetUnits: dictionary2);
	}

	public Dictionary<GraphNode, CustomPathNode> GetCustomPath(StarshipEntity starship, Vector3 position, Vector3 direction)
	{
		return GetPathData(starship, position, direction).grapNodes;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		StarshipEntity starshipEntity = null;
		if (target.IsPoint)
		{
			if (((CustomGridNodeBase)AstarPath.active.GetNearest(target.Point).node).TryGetUnit(out var unit))
			{
				starshipEntity = unit as StarshipEntity;
			}
		}
		else
		{
			starshipEntity = target.Entity as StarshipEntity;
		}
		if (!(ability.Caster is StarshipEntity starship))
		{
			return false;
		}
		Dictionary<StarshipEntity, int> item = GetPathData(starship, casterPosition, UnitPredictionManager.Instance.CurrentUnitDirection).targetUnits;
		if (!AllowAutotarget || !item.Any())
		{
			if (starshipEntity != null)
			{
				return item.ContainsKey(starshipEntity);
			}
			return false;
		}
		return true;
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		return LocalizedTexts.Instance.Reasons.NotAllowedCellToCast;
	}

	private void StartFires(StarshipEntity starship, AbilityExecutionContext context, StarshipEntity unit)
	{
		if (m_FirestartingFeature == null || !starship.Facts.Contains(m_FirestartingFeature.Get()))
		{
			return;
		}
		int num = unit.Blueprint.Size switch
		{
			Size.Frigate_1x2 => 2, 
			Size.Large => 2, 
			Size.Cruiser_2x4 => 3, 
			Size.GrandCruiser_3x6 => 4, 
			_ => 1, 
		};
		using (context.GetDataScope(unit.ToITargetWrapper()))
		{
			for (int i = 0; i < num; i++)
			{
				RepeatedBySizeActionsOnTarget?.Run();
			}
		}
	}

	private void RamDamage(StarshipEntity starship, StarshipEntity target, int extraRunup, float damageMod, RamDamagePrediction rp = null)
	{
		int num = 5;
		if (starship.Facts.Contains(m_DamageBonusTalent?.Get()))
		{
			num *= 2;
		}
		RamDamageBase(starship, target, extraRunup, num, damageMod, rp);
	}

	public static void RamDamageBase(StarshipEntity starship, StarshipEntity target, int extraRunup, int runupBonusPercent, float damageMod, RamDamagePrediction rp = null)
	{
		int num = RamBaseDmgBySize(starship);
		num += num * extraRunup * runupBonusPercent / 100 + starship.Hull.ProwRam.BonusDamage;
		bool valueOrDefault = (starship.Shields?.VoidShieldGenerator?.offOnRam).GetValueOrDefault();
		ProcessRamDamage(starship, target, num, damageMod, valueOrDefault, rp?.targetDamage, rp?.targetShields);
		int dmg2 = Math.Max(0, RamBaseDmgBySize(target) * 4 / 5 - starship.Hull.ProwRam.SelfDamageDeduction);
		float dmgMod2 = 1f + (from rm in starship.Facts.GetComponents<StarshipRamModifiers>()
			select rm.DamageReturningMod).DefaultIfEmpty(0f).Sum();
		ProcessRamDamage(target, starship, dmg2, dmgMod2, valueOrDefault, rp?.selfDamage, rp?.selfShields);
		static void ProcessRamDamage(StarshipEntity source, StarshipEntity target, int dmg, float dmgMod, bool ignoreShields, DamagePredictionData dp, ShieldDamageData sp)
		{
			dmg = Mathf.RoundToInt((float)dmg * dmgMod);
			bool flag = sp != null;
			StarshipHitLocation resultHitLocation = Rulebook.Trigger(new RuleStarshipCalculateHitLocation(source, target)).ResultHitLocation;
			int maxDamage = 0;
			RuleStarshipRollShieldAbsorption ruleStarshipRollShieldAbsorption = null;
			if (!ignoreShields)
			{
				ruleStarshipRollShieldAbsorption = new RuleStarshipRollShieldAbsorption(source, target, dmg, DamageType.Ram, resultHitLocation);
				ruleStarshipRollShieldAbsorption.Trigger(flag);
				if (ruleStarshipRollShieldAbsorption.ResultAbsorbedDamage > 0)
				{
					(int absorbedDamage, int shieldStrengthLoss) tuple = target.Shields.Absorb(ruleStarshipRollShieldAbsorption.ResultHitLocation, ruleStarshipRollShieldAbsorption.ResultAbsorbedDamage, DamageType.Ram, flag);
					int item = tuple.absorbedDamage;
					maxDamage = tuple.shieldStrengthLoss;
					dmg -= item;
				}
			}
			RuleStarshipCalculateDamageForTarget ruleStarshipCalculateDamageForTarget = Rulebook.Trigger(new RuleStarshipCalculateDamageForTarget(source, target, dmg, dmg, DamageType.Ram, isAEAttack: false, resultHitLocation));
			if (flag)
			{
				dp.MinDamage = ruleStarshipCalculateDamageForTarget.ResultDamage.MinValue;
				dp.MaxDamage = ruleStarshipCalculateDamageForTarget.ResultDamage.MaxValue;
				sp.MinDamage = (sp.MaxDamage = maxDamage);
				sp.CurrentShield = ruleStarshipRollShieldAbsorption?.ResultShields ?? target.Shields.ShieldsSum;
				sp.MaxShield = ruleStarshipRollShieldAbsorption?.ResultMaxShields ?? target.Shields.ShieldsMaxSum;
			}
			else
			{
				Rulebook.Trigger(new RuleDealDamage(source, target, ruleStarshipCalculateDamageForTarget.ResultDamage));
			}
		}
		static int RamBaseDmgBySize(StarshipEntity starship)
		{
			return starship.Blueprint.Size switch
			{
				Size.Raider_1x1 => 35, 
				Size.Frigate_1x2 => 50, 
				Size.Large => 65, 
				Size.Cruiser_2x4 => 80, 
				_ => 120, 
			};
		}
	}

	public void EvaluateRamPrediction(StarshipEntity starship, Vector3 casterPosition, StarshipEntity target, RamDamagePrediction rp)
	{
		Dictionary<StarshipEntity, int> item = GetPathData(starship, casterPosition, UnitPredictionManager.Instance.CurrentUnitDirection).targetUnits;
		if (item.Any())
		{
			if (target == null || target == starship)
			{
				target = item.First().Key;
			}
			if (item.TryGetValue(target, out var value))
			{
				rp.ramTarget = target;
				RamDamage(starship, target, value - minDistance, 1f, rp);
			}
		}
	}
}
