using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
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
			yield break;
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
		TargetWrapper launcher = new TargetWrapper(position, null, context.Caster);
		Projectile projectile = new ProjectileLauncher(projectileBlueprint, launcher, projectileTarget).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).Index(attackLine.Index)
			.MisdirectionOffset(projectileMisdirectionOffset)
			.IsCoverHit(isCoverHit)
			.Launch();
		attackLine.Projectile = projectile;
		Debug.DrawLine(vector3Position, vector3Position2, Color.yellow);
		yield return null;
		AbilityProjectileAttackLine.HitData[] array2 = hits;
		foreach (AbilityProjectileAttackLine.HitData hitData2 in array2)
		{
			foreach (AbilityDeliveryTarget item in HandleHit(attackLine, hitData2))
			{
				yield return item;
			}
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
		Dictionary<CustomGridNodeBase, PatternCellDataAccumulator> dictionary = new Dictionary<CustomGridNodeBase, PatternCellDataAccumulator>();
		List<RuleCalculateScatterShotHitDirectionProbability> scatterShotHitDirectionProbabilities = ability.ScatterShotHitDirectionProbabilities;
		HashSet<BaseUnitEntity> hashSet = TempHashSet.Get<BaseUnitEntity>();
		List<float> list = TempList.Get<float>();
		list.Capacity = scatterShotHitDirectionProbabilities.Count;
		for (int i = 0; i < array.Length; i++)
		{
			List<CustomGridNodeBase> list2 = array[i];
			int num = Mathf.Abs(i - 2);
			list.Clear();
			foreach (RuleCalculateScatterShotHitDirectionProbability item in scatterShotHitDirectionProbabilities)
			{
				List<float> list3 = list;
				list3.Add(num switch
				{
					0 => item.ResultMainLine, 
					1 => (float)item.ResultScatterNear / 2f, 
					_ => (float)item.ResultScatterFar / 2f, 
				} / 100f);
			}
			float[] array2 = list.ToArray();
			hashSet.Clear();
			foreach (CustomGridNodeBase item2 in list2)
			{
				BaseUnitEntity unit = item2.GetUnit();
				if (coveredTargetsOnly && unit == null)
				{
					continue;
				}
				float num2 = 1f;
				float coverProbability = 0f;
				float evasionProbability = 0f;
				if (unit == null)
				{
					num2 = 0f;
					goto IL_01a8;
				}
				using (ProfileScope.New("IsNodeAffected"))
				{
					if (!IsNodeAffected(ability, bestShootingPosition, item2, stepHeightBetweenCells) || ability.Caster.IsUnitPositionContainsNode(bestShootingPosition.Vector3Position, item2))
					{
						continue;
					}
					goto IL_01a8;
				}
				IL_01a8:
				if (unit != null)
				{
					hashSet.Add(unit);
					int direction = CustomGraphHelper.GuessDirection((bestShootingPosition.Vector3Position - item2.Vector3Position).normalized);
					LosDescription cellCoverStatus = LosCalculations.GetCellCoverStatus(item2, direction);
					coverProbability = cellCoverStatus.CoverType switch
					{
						LosCalculations.CoverType.None => 0f, 
						LosCalculations.CoverType.Invisible => 1f, 
						_ => (float)Rulebook.Trigger(new RuleCalculateCoverHitChance(ability.Caster, unit, ability.Data, cellCoverStatus, null)).ResultChance / 100f, 
					};
					num2 = (unit.IsDead ? 1f : ability.CalculateDodgeChanceCached((UnitEntity)unit, cellCoverStatus));
				}
				if (unit is StarshipEntity target)
				{
					evasionProbability = Rulebook.Trigger(new RuleStarshipCalculateHitChances((StarshipEntity)ability.Caster, target, ability.StarshipWeapon)).ResultHitChance;
				}
				Accumulate(dictionary, item2, array2, num2, coverProbability, evasionProbability, num == 0);
				for (int j = 0; j < array2.Length; j++)
				{
					if (unit != null)
					{
						array2[j] *= num2;
					}
				}
			}
		}
		return new OrientedPatternData(dictionary, bestShootingPosition);
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
