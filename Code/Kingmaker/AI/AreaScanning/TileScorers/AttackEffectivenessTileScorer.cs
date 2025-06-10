using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.AI.Learning;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.AI.AreaScanning.TileScorers;

public class AttackEffectivenessTileScorer : ProtectionTileScorer
{
	private static readonly float[] fireCoverValues = new float[4] { 1f, 0.02f, 0.0004f, 0f };

	private Dictionary<CustomGridNodeBase, List<MechanicEntity>> AffectedTargetsCache = new Dictionary<CustomGridNodeBase, List<MechanicEntity>>();

	private UnitDataStorage AiCollectedDataStorage => Game.Instance.Player.AiCollectedDataStorage;

	public ScoreSet CalculateAttackScore(DecisionContext context, AbilityInfo abilityInfo, MechanicEntity target, int distance, LosCalculations.CoverType los)
	{
		ScoreSet result = default(ScoreSet);
		result.Set(ScoreType.EnemyCoverScore, new Score(fireCoverValues[(int)los]));
		result.Set(ScoreType.EffectiveDistanceScore, new Score((distance <= abilityInfo.effectiveRange) ? 1f : 0f));
		result.Set(ScoreType.EnemyThreatScore, CalculateEnemyTargetThreatScore(context, target));
		result.Set(ScoreType.PriorityScore, new Score(context.Unit.Brain.GetTargetPriority(abilityInfo.ability, target)));
		PartHealth healthOptional = target.GetHealthOptional();
		result.Set(ScoreType.EnemyHPLeftScore, new Score((healthOptional != null) ? (1f / (float)healthOptional.HitPointsLeft) : 0f));
		result.Set(ScoreType.ClosinessScore, new Score((float)(abilityInfo.maxRange - distance) / (float)abilityInfo.maxRange));
		return result;
	}

	public override GraphNode GetHighestScoreNode(DecisionContext context, List<CustomGridNodeBase> nodes, ScoreOrder scoreOrder = null)
	{
		AbilityInfo abilityInfo = context.AbilityInfo;
		if (abilityInfo == null)
		{
			if (scoreOrder != null && scoreOrder.Order.First() == ScoreType.BodyGuardScore)
			{
				List<CustomGridNodeBase> list = TempList.Get<CustomGridNodeBase>();
				list.AddRange(nodes);
				ScoreOrder scoreOrder2 = new ScoreOrder();
				foreach (object value in Enum.GetValues(typeof(ScoreType)))
				{
					scoreOrder2.SetFactor((ScoreType)value, ScoreFactor.Ignored);
				}
				scoreOrder2.SetFactor(ScoreType.BodyGuardScore, ScoreFactor.Default);
				return base.GetHighestScoreNode(context, list, scoreOrder2);
			}
			return null;
		}
		List<CustomGridNodeBase> list2 = TempList.Get<CustomGridNodeBase>();
		if (scoreOrder == null || scoreOrder.Order.First() != ScoreType.BodyGuardScore)
		{
			AbilityTargetSelector targetSelector = abilityInfo.GetAbilityTargetSelector();
			list2.AddRange(nodes.Where((CustomGridNodeBase n) => targetSelector.HasPossibleTarget(context, n)));
		}
		else
		{
			list2.AddRange(nodes);
		}
		AffectedTargetsCache.Clear();
		return base.GetHighestScoreNode(context, list2, scoreOrder);
	}

	private List<MechanicEntity> GetTargets(DecisionContext context, CustomGridNodeBase node)
	{
		if (!AffectedTargetsCache.TryGetValue(node, out var value))
		{
			AbilityInfo abilityInfo = context.AbilityInfo;
			AbilityTargetSelector abilityTargetSelector = abilityInfo.GetAbilityTargetSelector();
			TargetWrapper targetWrapper = abilityTargetSelector.SelectTarget(context, node);
			value = new List<MechanicEntity>(abilityTargetSelector.AffectedTargets);
			if (value.Count == 0 && targetWrapper?.Entity != null)
			{
				value.Add(targetWrapper.Entity);
			}
			if (value.Count > 0)
			{
				List<TargetInfo> availableTargets = context.GetAvailableTargets(abilityInfo.ability);
				value = value.Where((MechanicEntity t) => availableTargets.Contains((TargetInfo u) => u.Entity == t)).ToList();
			}
			AffectedTargetsCache[node] = value;
		}
		return value;
	}

	protected override Score CalculateEnemyCoverScore(DecisionContext context, CustomGridNodeBase node)
	{
		Score result = default(Score);
		AbilityData abilityData = context.ConsideringAbility ?? context.Ability;
		foreach (MechanicEntity target in GetTargets(context, node))
		{
			LosCalculations.CoverType coverType = LosCalculations.CoverType.None;
			if (abilityData.NeedLoS)
			{
				CustomGridNodeBase end = (CustomGridNodeBase)(GraphNode)target.CurrentNode;
				coverType = LosCalculations.GetWarhammerLos(LosCalculations.GetBestShootingNode(node, context.Unit.SizeRect, end, target?.SizeRect ?? default(IntRect)), context.Unit.SizeRect, end, target?.SizeRect ?? default(IntRect));
			}
			result += new Score(fireCoverValues[(int)coverType]);
		}
		return result;
	}

	protected override Score CalculateEffectiveDistanceScore(DecisionContext context, CustomGridNodeBase node)
	{
		Score result = default(Score);
		AbilityInfo abilityInfo = context.AbilityInfo;
		foreach (MechanicEntity target in GetTargets(context, node))
		{
			int num = target.DistanceToInCells(node.Vector3Position);
			result += new Score((num <= abilityInfo.effectiveRange) ? 1f : 0f);
		}
		return result;
	}

	protected override Score CalculateEnemyThreatScore(DecisionContext context, CustomGridNodeBase node)
	{
		Score result = default(Score);
		foreach (MechanicEntity target in GetTargets(context, node))
		{
			result += CalculateEnemyTargetThreatScore(context, target);
		}
		return result;
	}

	protected override Score CalculatePriorityScore(DecisionContext context, CustomGridNodeBase node)
	{
		Score result = default(Score);
		foreach (MechanicEntity target in GetTargets(context, node))
		{
			result += new Score(context.GetTargetPriority(target));
		}
		return result;
	}

	protected override Score CalculateEnemyHPLeftScore(DecisionContext context, CustomGridNodeBase node)
	{
		Score result = default(Score);
		foreach (MechanicEntity target in GetTargets(context, node))
		{
			PartHealth healthOptional = target.GetHealthOptional();
			result += new Score((healthOptional != null) ? (1f / (float)healthOptional.HitPointsLeft) : 0f);
		}
		return result;
	}

	protected override Score CalculateClosinessScore(DecisionContext context, CustomGridNodeBase node)
	{
		Score result = default(Score);
		AbilityInfo abilityInfo = context.AbilityInfo;
		foreach (MechanicEntity target in GetTargets(context, node))
		{
			int num = target.DistanceToInCells(node.Vector3Position);
			result += new Score((float)(abilityInfo.maxRange - num) / (float)abilityInfo.maxRange);
		}
		return result;
	}

	protected override Score CalculateBodyGuardScore(DecisionContext context, CustomGridNodeBase node)
	{
		UnitPartBodyGuard optional = context.Unit.GetOptional<UnitPartBodyGuard>();
		if (optional == null || optional.Defendant == null || optional.Defendant.IsDeadOrUnconscious)
		{
			return default(Score);
		}
		float num = optional.Defendant.DistanceToInCells(node.Vector3Position);
		return new Score((num == 0f) ? 2f : (1f / num));
	}

	private Score CalculateEnemyTargetThreatScore(DecisionContext context, MechanicEntity entity)
	{
		float num = 0f;
		if (!(entity is BaseUnitEntity baseUnitEntity))
		{
			return new Score(num);
		}
		int threatRange = AiCollectedDataStorage[baseUnitEntity].AttackDataCollection.GetThreatRange();
		float actionPointsBlue = baseUnitEntity.CombatState.ActionPointsBlue;
		int num2 = context.Unit.DistanceToInCells(baseUnitEntity);
		if (num2 <= threatRange)
		{
			num = 1f;
		}
		else if ((float)num2 <= (float)threatRange + actionPointsBlue)
		{
			num = 0.5f;
		}
		return new Score(num);
	}
}
