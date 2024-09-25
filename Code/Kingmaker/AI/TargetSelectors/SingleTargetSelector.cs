using Kingmaker.AI.AreaScanning.Scoring;
using Kingmaker.AI.AreaScanning.TileScorers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;

namespace Kingmaker.AI.TargetSelectors;

public class SingleTargetSelector : AbilityTargetSelector
{
	private readonly AttackEffectivenessTileScorer m_AttackScorer = new AttackEffectivenessTileScorer();

	public SingleTargetSelector(AbilityInfo abilityInfo)
		: base(abilityInfo)
	{
	}

	public override bool HasPossibleTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		foreach (TargetInfo availableTarget in context.GetAvailableTargets(AbilityInfo.ability))
		{
			if (IsValidTarget(availableTarget.Entity) && AbilityInfo.ability.CanTargetFromNode(casterNode, availableTarget.Node, availableTarget.Entity, out var _, out var _))
			{
				return true;
			}
		}
		return false;
	}

	public override TargetWrapper SelectTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		ScoreSet s = default(ScoreSet);
		MechanicEntity mechanicEntity = null;
		foreach (TargetInfo availableTarget in context.GetAvailableTargets(AbilityInfo.ability))
		{
			if (IsValidTarget(availableTarget.Entity) && AbilityInfo.ability.CanTargetFromNode(casterNode, availableTarget.Node, availableTarget.Entity, out var distance, out var los))
			{
				ScoreSet scoreSet = m_AttackScorer.CalculateAttackScore(context, AbilityInfo, availableTarget.Entity, distance, los);
				if (context.ScoreOrder.Compare(scoreSet, s) > 0)
				{
					s = scoreSet;
					mechanicEntity = availableTarget.Entity;
					base.AffectedTargets.Clear();
					base.AffectedTargets.Add(mechanicEntity);
				}
			}
		}
		base.SelectedTarget = ((mechanicEntity != null) ? new TargetWrapper(mechanicEntity) : null);
		return base.SelectedTarget;
	}
}
