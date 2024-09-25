using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public static class AbilityProjectileAttackLineHelper
{
	private struct AttackResultData
	{
		[CanBeNull]
		public RulePerformAttack Rule;

		public bool IsHit;

		public bool IsCoverHit;

		public bool IsTargetPenetrated;

		[CanBeNull]
		public MechanicEntity TargetCoverEntity;
	}

	private static float s_MissTargetDistance = 30f;

	private const int VerticalDeviationLimit = 1;

	public static IEnumerator<AbilityDeliveryTarget> DeliverLine(AbilityExecutionContext context, BlueprintProjectile projectileBlueprint, AbilityProjectileAttackLine attackLine)
	{
		TargetWrapper targetWrapper = null;
		while (true)
		{
			CustomGridNodeBase fromNode = attackLine.FromNode;
			AbilityProjectileAttackLine.HitData[] hits;
			using (ProfileScope.New("CalculateHits"))
			{
				hits = attackLine.CalculateHits().ToArray();
			}
			ReadonlyList<CustomGridNodeBase> nodes = attackLine.Nodes;
			if (nodes.Count < 1)
			{
				PFLog.Default.ErrorWithReport($"Can't make attack {attackLine.Index}: projectile path is empty");
				break;
			}
			Vector3 vector3Position = fromNode.Vector3Position;
			Vector3 vector3Position2 = nodes[nodes.Count - 1].Vector3Position;
			Vector3 position = context.Caster.Position;
			TargetWrapper projectileTarget = GetProjectileTarget(context, attackLine, hits);
			Vector3 projectileMisdirectionOffset = GetProjectileMisdirectionOffset(position, projectileTarget.Point, 0.15f);
			bool isCoverHit = false;
			if (hits.Length != 0)
			{
				AbilityProjectileAttackLine.HitData[] array = hits;
				for (int i = 0; i < array.Length; i++)
				{
					AbilityProjectileAttackLine.HitData hitData = array[i];
					if (hitData.RollPerformAttackRule.ResultIsCoverHit)
					{
						isCoverHit = hitData.RollPerformAttackRule.ResultIsCoverHit;
						break;
					}
				}
			}
			TargetWrapper targetWrapper2 = ((targetWrapper != null) ? targetWrapper : new TargetWrapper(position, null, context.Caster));
			int value = context.Ability.RangeCells;
			if (hits.Length != 0)
			{
				AbilityProjectileAttackLine.HitData[] array = hits;
				for (int i = 0; i < array.Length; i++)
				{
					AbilityProjectileAttackLine.HitData hitData2 = array[i];
					if (hitData2.IsRedirecting)
					{
						isCoverHit = true;
						value = WarhammerGeometryUtils.DistanceToInCells(targetWrapper2.Point, default(IntRect), hitData2.Entity.Center, default(IntRect));
						break;
					}
				}
			}
			Projectile projectile = new ProjectileLauncher(projectileBlueprint, targetWrapper2, projectileTarget).Ability(context.Ability).MaxRangeCells(value).Index(attackLine.Index)
				.MisdirectionOffset(projectileMisdirectionOffset)
				.IsCoverHit(isCoverHit)
				.Launch();
			attackLine.Projectile = projectile;
			Debug.DrawLine(vector3Position, vector3Position2, Color.yellow);
			yield return null;
			AbilityProjectileAttackLine.HitData[] array2 = hits;
			foreach (AbilityProjectileAttackLine.HitData hitData3 in array2)
			{
				foreach (AbilityDeliveryTarget item in HandleHit(attackLine, hitData3))
				{
					yield return item;
				}
			}
			if (hits.Length == 0 || !hits.Last().IsRedirecting)
			{
				break;
			}
			MechanicEntity deflector = hits.Last().Entity;
			if (deflector == null)
			{
				break;
			}
			List<MechanicEntity> list = (from p in Game.Instance.State.AllUnits
				where !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && p.Health.HitPointsLeft > 0
				where p.Facts.GetComponents<WarhammerDeflectionTarget>().Any((WarhammerDeflectionTarget c) => c.Caster == deflector)
				select p).Cast<MechanicEntity>().ToList();
			list.Remove(hits.Last().Entity);
			list.RemoveAll((MechanicEntity p) => !p.IsEnemy(deflector));
			list.RemoveAll((MechanicEntity p) => !deflector.HasLOS(p));
			list.RemoveAll((MechanicEntity p) => deflector.DistanceToInCells(p) > context.Ability.RangeCells);
			if (!list.Empty())
			{
				MechanicEntity target = list.MinBy((MechanicEntity p) => deflector.DistanceToInCells(p));
				(ReadonlyList<CustomGridNodeBase>, CustomGridNodeBase, CustomGridNodeBase) tuple = AbilityProjectileAttack.CollectNodes((CustomGridNodeBase)deflector.CurrentNode.node, target, context.Ability.RangeCells);
				attackLine = new AbilityProjectileAttackLine(attackLine.ProjectileAttack, attackLine.Index, tuple.Item2, tuple.Item3, tuple.Item1, attackLine.WeaponAttackDamageDisabled, disableDodgeForAlly: true);
				targetWrapper = new TargetWrapper(deflector.Center, null, null);
				hits = null;
				continue;
			}
			break;
		}
	}

	private static TargetWrapper GetProjectileTarget(AbilityExecutionContext context, AbilityProjectileAttackLine attackLine, AbilityProjectileAttackLine.HitData[] hits)
	{
		CustomGridNodeBase customGridNodeBase = attackLine.Nodes.LastOrDefault((CustomGridNodeBase x) => IsNodeAffected(null, attackLine.FromNode, x, attackLine.StepHeight)) ?? attackLine.Nodes.Last();
		AbilityProjectileAttackLine.HitData hitData = hits.LastItem();
		AbilityProjectileAttackLine.HitData hitData2 = hits.LastItem((AbilityProjectileAttackLine.HitData i) => i.Entity is UnitEntity && i.RollPerformAttackRule.ResultIsHit);
		Vector3 vector3Position;
		if (hitData2.Empty)
		{
			if (!hitData.Empty && hitData.RollDamageRule?.ResultOverpenetration == null && hitData.RollPerformAttackRule.ResultIsHit)
			{
				vector3Position = hitData.Node.Vector3Position;
				vector3Position.y = customGridNodeBase.Vector3Position.y + 1f;
				return vector3Position;
			}
			Vector3 eyePosition = context.Caster.EyePosition;
			AbilityProjectileAttackLine.HitData hitData3 = hits.LastItem((AbilityProjectileAttackLine.HitData i) => i.Entity is UnitEntity);
			if (hitData3.Empty)
			{
				vector3Position = customGridNodeBase.Vector3Position;
				vector3Position.y = attackLine.ToNode.Vector3Position.y + 1f;
				eyePosition.y = vector3Position.y;
				return eyePosition + (vector3Position - eyePosition).normalized * s_MissTargetDistance;
			}
			if (TryGetTargetPointByRandomLocator(hitData2.Entity, context, hitData.Node, out vector3Position))
			{
				return vector3Position;
			}
			vector3Position = customGridNodeBase.Vector3Position;
			vector3Position.y = hitData3.Node.Vector3Position.y + 1f;
			eyePosition.y = vector3Position.y;
			return eyePosition + (vector3Position - eyePosition).normalized * s_MissTargetDistance;
		}
		if (hitData.RollDamageRule?.ResultOverpenetration == null && hitData.RollPerformAttackRule.ResultIsHit)
		{
			return hitData2.Entity;
		}
		if (TryGetTargetPointByRandomLocator(hitData2.Entity, context, hitData.Node, out vector3Position))
		{
			return vector3Position;
		}
		vector3Position = hitData.Node.Vector3Position;
		vector3Position.y = customGridNodeBase.Vector3Position.y + 1f;
		return vector3Position;
	}

	private static bool TryGetTargetPointByRandomLocator(MechanicEntity unit, AbilityExecutionContext context, CustomGridNodeBase lastHitNode, out Vector3 result)
	{
		if (!(unit is UnitEntity unitEntity))
		{
			result = default(Vector3);
			return false;
		}
		FxBone fxBone = ObjectExtensions.Or(unitEntity.View.ParticlesSnapMap, null)?.GetLocators(FxRoot.Instance.LocatorGroupTorso).Random(PFStatefulRandom.UnitLogic.Abilities);
		if (fxBone == null)
		{
			result = default(Vector3);
			return false;
		}
		Vector3 normalized = (fxBone.Transform.position - context.Caster.EyePosition).normalized;
		float magnitude = (lastHitNode.Vector3Position - context.Caster.Position).magnitude;
		result = context.Caster.EyePosition + normalized * magnitude;
		return true;
	}

	private static IEnumerable<AbilityDeliveryTarget> HandleHit(AbilityProjectileAttackLine attackLine, AbilityProjectileAttackLine.HitData hitData)
	{
		CustomGridNodeBase node = hitData.Node;
		AbilityExecutionContext context = attackLine.Context;
		Projectile projectile = attackLine.Projectile;
		CustomGridNodeBase fromNode = attackLine.FromNode;
		Vector3 casterPosition = fromNode.Vector3Position;
		ReadonlyList<CustomGridNodeBase> nodes = attackLine.Nodes;
		Vector3 targetPosition = nodes[nodes.Count - 1].Vector3Position;
		float distance = projectile.Distance(node.Vector3Position, context.Caster.Position);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			Debug.DrawLine(casterPosition, targetPosition, Color.yellow);
			yield return null;
		}
		Debug.DrawLine(casterPosition, targetPosition, Color.yellow);
		Debug.DrawLine(node.Vector3Position, node.Vector3Position + Vector3.up * 3f, Color.yellow);
		MechanicEntity currentTarget = hitData.Entity;
		if (currentTarget != null)
		{
			AttackResultData attack = MakeAttack(context, attackLine, hitData, projectile);
			if (attack.TargetCoverEntity != null)
			{
				Debug.DrawLine(node.Vector3Position, attack.TargetCoverEntity.Position + Vector3.up * 3f, Color.red);
				yield return new AbilityDeliveryTarget(attack.TargetCoverEntity)
				{
					AttackRule = attack.Rule,
					Projectile = projectile
				};
			}
			Debug.DrawLine(node.Vector3Position, currentTarget.Position + Vector3.up * 3f, Color.red);
			yield return new AbilityDeliveryTarget(currentTarget)
			{
				AttackRule = attack.Rule,
				Projectile = projectile
			};
		}
	}

	private static AttackResultData MakeAttack(AbilityExecutionContext context, AbilityProjectileAttackLine attackLine, AbilityProjectileAttackLine.HitData hitData, Projectile projectile)
	{
		if (attackLine.ProjectileAttack.AttacksDisabled || hitData.Entity == null)
		{
			AttackResultData result = default(AttackResultData);
			result.IsHit = true;
			result.IsTargetPenetrated = !attackLine.ProjectileAttack.OverpenetrationDisabled;
			return result;
		}
		RulePerformAttack rulePerformAttack = new RulePerformAttack(context.Caster, hitData.Entity, context.Ability, attackLine.Index, attackLine.WeaponAttackDamageDisabled, attackLine.DodgeForAllyDisabled, hitData.RollPerformAttackRule, hitData.RollDamageRule)
		{
			IsOverpenetration = hitData.IsOverpenetration,
			Projectile = projectile,
			Reason = context
		};
		rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(attackLine.Nodes);
		using RulePerformDodge.DelayedDodge delayedDodge = ContextData<RulePerformDodge.DelayedDodge>.Request();
		context.TriggerRule(rulePerformAttack);
		foreach (var dodgedUnit in delayedDodge.DodgedUnits)
		{
			attackLine.ProjectileAttack.DodgedUnits.Add(dodgedUnit);
		}
		AttackResult result2 = rulePerformAttack.Result;
		bool isHit = result2 == AttackResult.Hit || result2 == AttackResult.RighteousFury;
		bool isCoverHit = rulePerformAttack.Result == AttackResult.CoverHit;
		bool isTargetPenetrated = rulePerformAttack.ResultOverpenetrationDamage != null;
		MechanicEntity resultCoverEntity = rulePerformAttack.RollPerformAttackRule.ResultCoverEntity;
		AttackResultData result = default(AttackResultData);
		result.Rule = rulePerformAttack;
		result.IsHit = isHit;
		result.IsCoverHit = isCoverHit;
		result.IsTargetPenetrated = isTargetPenetrated;
		result.TargetCoverEntity = resultCoverEntity;
		return result;
	}

	public static bool IsNodeAffected(IAbilityDataProviderForPattern ability, CustomGridNodeBase fromNode, CustomGridNodeBase targetNode, float stepHeight)
	{
		using (ProfileScope.New("HasLos"))
		{
			if (ability != null)
			{
				if (!ability.HasLosCached(fromNode, targetNode))
				{
					return false;
				}
			}
			else if (!LosCalculations.HasLos(fromNode, default(IntRect), targetNode, default(IntRect)))
			{
				return false;
			}
		}
		int num = Mathf.Max(Mathf.Abs(fromNode.XCoordinateInGrid - targetNode.XCoordinateInGrid), Mathf.Abs(fromNode.ZCoordinateInGrid - targetNode.ZCoordinateInGrid));
		float num2 = fromNode.Vector3Position.y + (float)num * stepHeight;
		if (Mathf.Abs(targetNode.Vector3Position.y - num2) <= 1f)
		{
			return true;
		}
		foreach (DestructibleEntity destructibleEntity in Game.Instance.State.DestructibleEntities)
		{
			if (destructibleEntity.CanBeAttackedDirectly && destructibleEntity.GetOccupiedNodes().Contains(targetNode) && num2 >= destructibleEntity.Position.y - 1f && num2 <= targetNode.Vector3Position.y + 1f)
			{
				return true;
			}
		}
		return false;
	}

	public static float GetStepHeightBetweenCells(CustomGridNodeBase fromNode, CustomGridNodeBase toNode)
	{
		int num = Mathf.Max(Mathf.Abs(fromNode.XCoordinateInGrid - toNode.XCoordinateInGrid), Mathf.Abs(fromNode.ZCoordinateInGrid - toNode.ZCoordinateInGrid));
		return (toNode.Vector3Position.y - fromNode.Vector3Position.y) / (float)num;
	}

	private static Vector3 GetProjectileMisdirectionOffset(Vector3 from, Vector3 to, float radius)
	{
		Vector3 normalized = (from - to).normalized;
		Vector3 vector = new Vector3(normalized.z, 0f, normalized.x);
		Vector3 up = Vector3.up;
		return PFStatefulRandom.UnitLogic.Abilities.Range(0f - radius, radius) * vector + PFStatefulRandom.UnitLogic.Abilities.Range(0f - radius, radius) * up;
	}

	public static OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, bool coveredTargetsOnly = false)
	{
		CustomGridNodeBase bestShootingPosition = ability.GetBestShootingPosition(casterNode, new TargetWrapper(targetNode.Vector3Position));
		List<CustomGridNodeBase>[] array = GridPatterns.CalcScatterShot(bestShootingPosition, targetNode, ability.RangeCells);
		float stepHeightBetweenCells = GetStepHeightBetweenCells(bestShootingPosition, targetNode);
		int num = 0;
		List<CustomGridNodeBase>[] array2 = array;
		foreach (List<CustomGridNodeBase> list in array2)
		{
			num += list.Count;
		}
		Dictionary<CustomGridNodeBase, PatternCellDataAccumulator> dictionary = new Dictionary<CustomGridNodeBase, PatternCellDataAccumulator>(num);
		ReadonlyList<RuleCalculateScatterShotHitDirectionProbability> scatterShotHitDirectionProbabilities = ability.ScatterShotHitDirectionProbabilities;
		List<float> list2 = TempList.Get<float>();
		list2.IncreaseCapacity(scatterShotHitDirectionProbabilities.Count);
		for (int j = 0; j < array.Length; j++)
		{
			List<CustomGridNodeBase> list3 = array[j];
			int num2 = Mathf.Abs(j - 2);
			list2.Clear();
			foreach (RuleCalculateScatterShotHitDirectionProbability item in scatterShotHitDirectionProbabilities)
			{
				List<float> list4 = list2;
				list4.Add(num2 switch
				{
					0 => item.ResultMainLine, 
					1 => (float)item.ResultScatterNear / 2f, 
					_ => (float)item.ResultScatterFar / 2f, 
				} / 100f);
			}
			float[] array3 = list2.ToArray();
			foreach (CustomGridNodeBase item2 in list3)
			{
				BaseUnitEntity unit = item2.GetUnit();
				if ((coveredTargetsOnly && unit == null) || !GetCellData(dictionary, item2, ability, bestShootingPosition, stepHeightBetweenCells, out var dodgeChance, out var hitCoverProbability, out var evasionProbability))
				{
					continue;
				}
				Accumulate(dictionary, item2, array3, dodgeChance, hitCoverProbability, evasionProbability, num2 == 0);
				for (int k = 0; k < array3.Length; k++)
				{
					if (unit != null)
					{
						array3[k] *= dodgeChance;
					}
				}
			}
		}
		return new OrientedPatternData(dictionary, bestShootingPosition);
	}

	private static bool GetCellData(Dictionary<CustomGridNodeBase, PatternCellDataAccumulator> cache, CustomGridNodeBase cell, IAbilityDataProviderForPattern ability, CustomGridNodeBase effectiveShootingNode, float stepHeight, out float dodgeChance, out float hitCoverProbability, out float evasionProbability)
	{
		if (cache.TryGetValue(cell, out var value))
		{
			dodgeChance = value.DodgeProbability;
			hitCoverProbability = value.CoverProbability;
			evasionProbability = value.EvasionProbability;
			return true;
		}
		BaseUnitEntity unit = cell.GetUnit();
		dodgeChance = 1f;
		hitCoverProbability = 0f;
		evasionProbability = 0f;
		if (unit == null)
		{
			dodgeChance = 0f;
		}
		else
		{
			using (ProfileScope.New("IsNodeAffected"))
			{
				if (!IsNodeAffected(ability, effectiveShootingNode, cell, stepHeight) || ability.Caster.IsUnitPositionContainsNode(effectiveShootingNode.Vector3Position, cell))
				{
					dodgeChance = 0f;
					hitCoverProbability = 0f;
					evasionProbability = 0f;
					return false;
				}
			}
		}
		if (unit != null)
		{
			int direction = CustomGraphHelper.GuessDirection((effectiveShootingNode.Vector3Position - cell.Vector3Position).normalized);
			LosDescription cellCoverStatus = LosCalculations.GetCellCoverStatus(cell, direction);
			hitCoverProbability = cellCoverStatus.CoverType switch
			{
				LosCalculations.CoverType.None => 0f, 
				LosCalculations.CoverType.Invisible => 1f, 
				_ => (float)Rulebook.Trigger(new RuleCalculateCoverHitChance(ability.Caster, unit, ability.Data, cellCoverStatus, null)).ResultChance / 100f, 
			};
			dodgeChance = (unit.IsDead ? 1f : ability.CalculateDodgeChanceCached((UnitEntity)unit, cellCoverStatus));
		}
		if (unit is StarshipEntity target)
		{
			RuleStarshipCalculateHitChances ruleStarshipCalculateHitChances = Rulebook.Trigger(new RuleStarshipCalculateHitChances((StarshipEntity)ability.Caster, target, ability.StarshipWeapon));
			evasionProbability = ruleStarshipCalculateHitChances.ResultHitChance;
		}
		return true;
	}

	private static void Accumulate(Dictionary<CustomGridNodeBase, PatternCellDataAccumulator> nodesData, CustomGridNodeBase node, float[] initialProbability, float dodgeProbability, float coverProbability, float evasionProbability, bool mainCell)
	{
		if (nodesData.TryGetValue(node, out var value))
		{
			value.AddShotProbability(initialProbability, dodgeProbability, coverProbability, evasionProbability, mainCell);
		}
		else
		{
			value = new PatternCellDataAccumulator(initialProbability, dodgeProbability, coverProbability, evasionProbability, mainCell);
		}
		nodesData[node] = value;
	}
}
