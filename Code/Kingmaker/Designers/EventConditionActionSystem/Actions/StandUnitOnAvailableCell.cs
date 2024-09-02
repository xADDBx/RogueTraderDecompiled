using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Enums.Helper;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.View;
using Owlcat.QA.Validation;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("4d5cd73f38374345979f2107ab50f9f4")]
public class StandUnitOnAvailableCell : GameAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public override string GetDescription()
	{
		return $"Ставит юнита {Target} в ближайшую доступную ноду";
	}

	public override string GetCaption()
	{
		return "Stand " + Target?.GetCaption() + " on nearest cell";
	}

	protected override void RunAction()
	{
		AbstractUnitEntity value = Target.GetValue();
		Vector3 position = value.Position;
		IntRect rectForSize = SizePathfindingHelper.GetRectForSize(value.Blueprint.Size);
		WarhammerSingleNodeBlocker warhammerSingleNodeBlocker = ((value is BaseUnitEntity baseUnitEntity) ? baseUnitEntity.View.MovementAgent.Blocker : null);
		CustomGridNode nearestNodeXZUnwalkable = ObstacleAnalyzer.GetNearestNodeXZUnwalkable(value.Position);
		if (nearestNodeXZUnwalkable != null && WarhammerBlockManager.Instance.CanUnitStandOnNode(rectForSize, nearestNodeXZUnwalkable, warhammerSingleNodeBlocker))
		{
			position = nearestNodeXZUnwalkable.Vector3Position;
		}
		else
		{
			foreach (CustomGridNodeBase item in GridAreaHelper.GetNodesSpiralAround(nearestNodeXZUnwalkable, rectForSize, 2))
			{
				if (WarhammerBlockManager.Instance.CanUnitStandOnNode(rectForSize, item, warhammerSingleNodeBlocker))
				{
					position = item.Vector3Position;
					break;
				}
			}
		}
		warhammerSingleNodeBlocker?.Unblock();
		value.Movable.ForceHasMotion = true;
		value.Position = position;
		warhammerSingleNodeBlocker?.BlockAtCurrentPosition();
	}
}
