using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility;

namespace Kingmaker.AI.TargetSelectors.CustomSelectors;

public class SingleTargetSelectorByDistance : SingleTargetSelector
{
	public SingleTargetSelectorByDistance(AbilityInfo abilityInfo)
		: base(abilityInfo)
	{
	}

	public override TargetWrapper SelectTarget(DecisionContext context, CustomGridNodeBase casterNode)
	{
		MechanicEntity mechanicEntity = null;
		foreach (TargetInfo availableTarget in context.GetAvailableTargets(AbilityInfo.ability))
		{
			if (IsValidTarget(availableTarget.Entity) && AbilityInfo.ability.CanTargetFromNode(casterNode, availableTarget.Node, availableTarget.Entity, out var _, out var _))
			{
				if (mechanicEntity == null)
				{
					mechanicEntity = availableTarget.Entity;
				}
				else if (availableTarget.Entity.DistanceToInCells(casterNode.Vector3Position) > mechanicEntity.DistanceToInCells(casterNode.Vector3Position))
				{
					mechanicEntity = availableTarget.Entity;
				}
			}
		}
		base.AffectedTargets.Clear();
		base.AffectedTargets.Add(mechanicEntity);
		base.SelectedTarget = ((mechanicEntity != null) ? new TargetWrapper(mechanicEntity) : null);
		return base.SelectedTarget;
	}
}
