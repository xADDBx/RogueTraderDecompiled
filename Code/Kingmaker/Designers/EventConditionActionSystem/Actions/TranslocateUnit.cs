using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.Attributes;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[ComponentName("Actions/TranslocateUnit")]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(true)]
[TypeId("38b104786c153ae409ee91b85544a4a5")]
public class TranslocateUnit : GameAction
{
	private const int NOD_SEARCH_RADIUS = 2;

	[SerializeReference]
	public AbstractUnitEvaluator Unit;

	[AllowedEntityType(typeof(LocatorView))]
	[Tooltip("Locator View")]
	public EntityReference translocatePosition;

	[SerializeReference]
	public PositionEvaluator translocatePositionEvaluator;

	[SerializeField]
	[Tooltip("Ignore smart positioning and place exactly at target position")]
	private bool m_PrecisePosition;

	[SerializeField]
	private bool m_CopyRotation;

	[ShowIf("m_CopyRotation")]
	[SerializeReference]
	public FloatEvaluator translocateOrientationEvaluator;

	public override string GetCaption()
	{
		return "Translocate Unit";
	}

	public override void RunAction()
	{
		AbstractUnitEntity targetUnit = Unit.GetValue();
		if (translocatePositionEvaluator == null && translocatePosition == null)
		{
			return;
		}
		targetUnit.View.StopMoving();
		Vector3 position;
		if (translocatePositionEvaluator == null)
		{
			position = translocatePosition.FindView().ViewTransform.position;
			targetUnit.View.MovementAgent.Blocker.Unblock();
		}
		else
		{
			position = translocatePositionEvaluator.GetValue();
		}
		if (m_PrecisePosition)
		{
			MoveUnit(targetUnit, position);
			return;
		}
		CustomGridNodeBase nearestNodeXZ = position.GetNearestNodeXZ();
		if (nearestNodeXZ == null)
		{
			return;
		}
		if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(targetUnit, nearestNodeXZ) && nearestNodeXZ.Walkable)
		{
			nearestNodeXZ = GridAreaHelper.GetNodesSpiralAround(nearestNodeXZ, new IntRect(0, 0, 0, 0), 2).FirstOrDefault((CustomGridNodeBase node) => WarhammerBlockManager.Instance.CanUnitStandOnNode(targetUnit, node) && node.Walkable);
			if (nearestNodeXZ == null)
			{
				return;
			}
			position = nearestNodeXZ.Vector3Position;
		}
		MoveUnit(targetUnit, position);
	}

	private void MoveUnit(AbstractUnitEntity unit, Vector3 position)
	{
		unit.Position = position;
		unit.View.MovementAgent.Blocker.BlockAt(unit.Position);
		if (m_CopyRotation && translocateOrientationEvaluator != null)
		{
			unit.SetOrientation(translocateOrientationEvaluator.GetValue());
		}
		else if (m_CopyRotation)
		{
			unit.SetOrientation(translocatePosition.FindView().ViewTransform.rotation.eulerAngles.y);
		}
	}
}
