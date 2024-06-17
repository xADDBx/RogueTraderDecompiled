using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.AI.AreaScanning.TileScorers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View.Covers;
using UnityEngine;

namespace Kingmaker.AI.TargetSelectors;

public class ScatterShotTargetSelector : AbilityTargetSelector
{
	private struct HitChancesData
	{
		public MechanicEntity Target;

		public float HitChance;

		public override string ToString()
		{
			return $"{Target.Blueprint}: {HitChance}%";
		}
	}

	private readonly AttackEffectivenessTileScorer m_AttackScorer = new AttackEffectivenessTileScorer();

	private readonly List<HitChancesData> m_AbilityTargets = new List<HitChancesData>();

	public ScatterShotTargetSelector(AbilityInfo abilityInfo)
		: base(abilityInfo)
	{
	}

	public override bool HasPossibleTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		foreach (TargetInfo hatedTarget in context.HatedTargets)
		{
			if (IsValidTarget(hatedTarget.Entity) && AbilityInfo.CanTargetFromNodeCached(casterNode, hatedTarget.Node, hatedTarget.Entity, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public override TargetWrapper SelectTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		Vector3 vector3Position = casterNode.Vector3Position;
		ScoreSet bestScore = default(ScoreSet);
		CustomGridNodeBase customGridNodeBase = null;
		foreach (TargetInfo hatedTarget in context.HatedTargets)
		{
			if (IsValidTarget(hatedTarget.Entity) && AbilityInfo.CanTargetFromNodeCached(casterNode, hatedTarget.Node, hatedTarget.Entity, out var distance, out var los))
			{
				ScoreSet checkScore = m_AttackScorer.CalculateAttackScore(context, AbilityInfo, hatedTarget.Entity, distance, los);
				(bestScore, customGridNodeBase) = GetBestScatterShotTarget(context, casterNode, hatedTarget.Node, checkScore, bestScore, customGridNodeBase);
			}
		}
		bool flag = AbilityInfo.ability.TargetAnchor != AbilityTargetAnchor.Point;
		if (customGridNodeBase != null)
		{
			if (!customGridNodeBase.TryGetUnit(out var unit))
			{
				if (!flag)
				{
					return new TargetWrapper(customGridNodeBase.Vector3Position);
				}
				return null;
			}
			return new TargetWrapper(unit);
		}
		if (flag)
		{
			return null;
		}
		foreach (TargetInfo hatedTarget2 in context.HatedTargets)
		{
			if (!IsValidTarget(hatedTarget2.Entity))
			{
				continue;
			}
			var (customGridNodeBase2, customGridNodeBase3) = LosCalculations.GetOrthoNeighbours(hatedTarget2.Node, hatedTarget2.Node.Vector3Position - vector3Position);
			if (!AbilityInfo.CanTargetFromNodeCached(casterNode, hatedTarget2.Node, hatedTarget2.Entity, out var distance2, out var los2))
			{
				continue;
			}
			ScoreSet checkScore2 = m_AttackScorer.CalculateAttackScore(context, AbilityInfo, hatedTarget2.Entity, distance2, los2);
			if (customGridNodeBase2 != null && AbilityInfo.ability.CanTargetFromNode(casterNode, customGridNodeBase2, new TargetWrapper(customGridNodeBase2.Vector3Position), out var distance3, out var los3))
			{
				(bestScore, customGridNodeBase) = GetBestScatterShotTarget(context, casterNode, customGridNodeBase2, checkScore2, bestScore, customGridNodeBase);
				if (customGridNodeBase != null)
				{
					base.SelectedTarget = (customGridNodeBase.TryGetUnit(out var unit2) ? new TargetWrapper(unit2) : new TargetWrapper(customGridNodeBase.Vector3Position));
					return base.SelectedTarget;
				}
			}
			if (customGridNodeBase3 != null && AbilityInfo.ability.CanTargetFromNode(casterNode, customGridNodeBase3, new TargetWrapper(customGridNodeBase3.Vector3Position), out distance3, out los3))
			{
				(bestScore, customGridNodeBase) = GetBestScatterShotTarget(context, casterNode, customGridNodeBase3, checkScore2, bestScore, customGridNodeBase);
				if (customGridNodeBase != null)
				{
					base.SelectedTarget = (customGridNodeBase.TryGetUnit(out var unit3) ? new TargetWrapper(unit3) : new TargetWrapper(customGridNodeBase.Vector3Position));
					return base.SelectedTarget;
				}
			}
		}
		base.SelectedTarget = ((customGridNodeBase == null) ? null : (customGridNodeBase.TryGetUnit(out var unit4) ? new TargetWrapper(unit4) : new TargetWrapper(customGridNodeBase.Vector3Position)));
		return base.SelectedTarget;
	}

	private (ScoreSet bestScore, CustomGridNodeBase target) GetBestScatterShotTarget(DecisionContext context, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode, ScoreSet checkScore, ScoreSet bestScore, CustomGridNodeBase bestTarget)
	{
		bool flag = IsScatterShotRisky(context, casterNode, targetNode);
		if (context.ScoreOrder.Compare(checkScore, bestScore) > 0 && !flag)
		{
			base.AffectedTargets = m_AbilityTargets.Select((HitChancesData x) => x.Target).ToList();
			if (AbilityInfo.settings == null || base.AffectedTargets.Count((MechanicEntity x) => x.IsEnemy(context.Ability?.Caster)) >= AbilityInfo.settings?.MustHitTargetsCount)
			{
				return (bestScore: checkScore, target: targetNode);
			}
		}
		return (bestScore: bestScore, target: bestTarget);
	}

	public bool IsScatterShotRisky(DecisionContext context, CustomGridNodeBase casterNode, CustomGridNodeBase targetNode)
	{
		BaseUnitEntity unit = context.Unit;
		bool flag = !unit.IsPlayerEnemy;
		OrientedPatternData orientedPattern;
		using (ProfileScope.New("GetOrientedPattern"))
		{
			orientedPattern = AbilityInfo.patternProvider.GetOrientedPattern(AbilityInfo, casterNode, targetNode, coveredTargetsOnly: true);
		}
		using (ProfileScope.New("GatherAffectedTargetsData"))
		{
			GatherAffectedTargetsData(orientedPattern, unit.Brain.IsCarefulShooter);
		}
		float num = 0f;
		using (ProfileScope.New("CalculateScore"))
		{
			float hitUnintendedTargetPenalty = unit.Brain.HitUnintendedTargetPenalty;
			foreach (HitChancesData abilityTarget in m_AbilityTargets)
			{
				if (!IsTargetCounts(abilityTarget.Target))
				{
					continue;
				}
				if (unit.CombatGroup.IsEnemy(abilityTarget.Target))
				{
					num += abilityTarget.HitChance;
				}
				else if (unit.CombatGroup.IsAlly(abilityTarget.Target))
				{
					if (flag)
					{
						return true;
					}
					num -= hitUnintendedTargetPenalty * abilityTarget.HitChance;
				}
			}
		}
		return num <= 0f;
	}

	private void GatherAffectedTargetsData(OrientedPatternData pattern, bool isCarefulShooter)
	{
		m_AbilityTargets.Clear();
		foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.State.AllBaseAwakeUnits)
		{
			float num = 0f;
			bool flag = false;
			foreach (CustomGridNodeBase occupiedNode in allBaseAwakeUnit.GetOccupiedNodes())
			{
				if (pattern.TryGet(occupiedNode, out var data))
				{
					flag = true;
					num += (isCarefulShooter ? (data.InitialProbabilities[^1] * (1f - data.DodgeProbability) * (1f - data.CoverProbability)) : data.ProbabilitiesSum);
				}
			}
			if (flag)
			{
				m_AbilityTargets.Add(new HitChancesData
				{
					Target = allBaseAwakeUnit,
					HitChance = num
				});
			}
		}
	}
}
