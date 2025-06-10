using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Inspect;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UI.Sound;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickSurfaceDeploymentHandler : IClickEventHandler
{
	private const int PET_DISTANCE_RESTRICTION_IN_CELLS = 2;

	public PointerMode GetMode()
	{
		return PointerMode.Default;
	}

	public virtual HandlerPriorityResult GetPriority(GameObject gameObject, Vector3 worldPosition)
	{
		return new HandlerPriorityResult(GetPriorityInternal(gameObject, worldPosition));
	}

	private float GetPriorityInternal(GameObject gameObject, Vector3 worldPosition)
	{
		if (!gameObject)
		{
			return 0f;
		}
		if (!TurnController.IsInTurnBasedCombat() || !Game.Instance.TurnController.IsPreparationTurn || !Game.Instance.TurnController.IsDeploymentAllowed)
		{
			return 0f;
		}
		UnitEntityView componentNonAlloc = gameObject.GetComponentNonAlloc<UnitEntityView>();
		if (componentNonAlloc != null)
		{
			if (!componentNonAlloc.EntityData.IsDirectlyControllable)
			{
				return 1.5f;
			}
			return 2f;
		}
		if (!gameObject.IsLayerMask(Layers.WalkableMask))
		{
			return 0f;
		}
		return 1f;
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		UnitEntityView unit = gameObject.GetComponentNonAlloc<UnitEntityView>();
		if (button == 1 && InspectUnitsHelper.IsInspectAllow(unit.Or(null)?.EntityData))
		{
			EventBus.RaiseEvent(delegate(IUnitClickUIHandler h)
			{
				h.HandleUnitRightClick(unit.EntityData);
			});
			return false;
		}
		if (button != 0)
		{
			return false;
		}
		if (unit == null || (!unit.EntityData.IsDirectlyControllable && !unit.EntityData.IsPet))
		{
			if (PhotonManager.Ping.CheckPingCoop(delegate
			{
				if (unit == null)
				{
					PhotonManager.Ping.PingPosition(worldPosition);
				}
				else if (!unit.EntityData.IsDirectlyControllable)
				{
					PhotonManager.Ping.PingEntity(unit.EntityData);
				}
			}))
			{
				return false;
			}
			return TryDeployCurrentUnit(worldPosition);
		}
		if (Game.Instance.SelectionCharacter.SelectedUnitInUI.Value == unit.EntityData)
		{
			if (!unit.EntityData.Blueprint.Size.Is1x1() && worldPosition.GetNearestNodeXZUnwalkable() != unit.EntityData.Position.GetNearestNodeXZUnwalkable())
			{
				if (PhotonManager.Ping.CheckPingCoop(delegate
				{
					PhotonManager.Ping.PingPosition(worldPosition);
				}))
				{
					return false;
				}
				return TryDeployCurrentUnit(worldPosition);
			}
			if (PhotonManager.Ping.CheckPingCoop(delegate
			{
				PhotonManager.Ping.PingEntity(unit.EntityData);
			}))
			{
				return false;
			}
		}
		else
		{
			if (PhotonManager.Ping.CheckPingCoop(delegate
			{
				PhotonManager.Ping.PingEntity(unit.EntityData);
			}))
			{
				return false;
			}
			Game.Instance.SelectionCharacter.SetSelected(unit.EntityData);
		}
		return true;
	}

	private bool TryDeployCurrentUnit(Vector3 worldPosition)
	{
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnit.Value;
		if (!value.IsMyNetRole())
		{
			return false;
		}
		CustomGridNodeBase customGridNodeBase = UnitPathManager.Instance?.CurrentNode ?? worldPosition.GetNearestNodeXZUnwalkable();
		if (!CanDeployUnit(customGridNodeBase))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(LocalizedTexts.Instance.Reasons.PathBlocked, addToLog: false, WarningNotificationFormat.Attention);
			});
			return false;
		}
		UnitTeleportParams cmdParams = new UnitTeleportParams(customGridNodeBase.Vector3Position, isSynchronized: true);
		value.Commands.Run(cmdParams);
		UISounds.Instance.Sounds.Combat.PreparationTurnDeployUnit.Play();
		UnitPartPetOwner petOwner = value.GetOptional<UnitPartPetOwner>();
		if (petOwner != null && petOwner.PetUnit.DistanceToInCells(worldPosition, value.SizeRect) > 2)
		{
			UnitTeleportParams cmdParams2 = new UnitTeleportParams((from n in GridAreaHelper.GetNodesSpiralAround(customGridNodeBase, value.SizeRect, 2)
				where CanDeployUnit(n, petOwner.PetUnit, ignorePetRestriction: true)
				select n).MinBy((CustomGridNodeBase n) => petOwner.PetUnit.DistanceTo(n.Vector3Position)).Vector3Position, isSynchronized: true);
			petOwner.PetUnit.Commands.Run(cmdParams2);
		}
		return true;
	}

	public static bool CanDeployUnit(GraphNode node)
	{
		return CanDeployUnit(node, Game.Instance.SelectionCharacter.SelectedUnit.Value);
	}

	public static bool CanDeployUnit(GraphNode node, BaseUnitEntity unit, bool ignorePetRestriction = false)
	{
		if (!(node is CustomGridNode customGridNode))
		{
			return false;
		}
		Dictionary<Vector2Int, GraphNode> dictionary = new Dictionary<Vector2Int, GraphNode>();
		List<GraphNode> deploymentForbiddenArea = Game.Instance.UnitMovableAreaController.DeploymentForbiddenArea;
		object obj;
		if (deploymentForbiddenArea != null)
		{
			obj = Game.Instance.UnitMovableAreaController.CurrentUnitMovableArea?.Except(deploymentForbiddenArea);
		}
		else
		{
			IEnumerable<GraphNode> currentUnitMovableArea = Game.Instance.UnitMovableAreaController.CurrentUnitMovableArea;
			obj = currentUnitMovableArea;
		}
		IEnumerable<GraphNode> enumerable = (IEnumerable<GraphNode>)obj;
		if (enumerable != null)
		{
			foreach (GraphNode item in enumerable)
			{
				if (item is CustomGridNode customGridNode2)
				{
					dictionary[customGridNode2.CoordinatesInGrid] = item;
				}
			}
		}
		if (!unit.CanMove)
		{
			return false;
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = unit.Position.GetNearestNodeXZUnwalkable();
		if (nearestNodeXZUnwalkable == null)
		{
			return false;
		}
		Vector2Int coordinatesInGrid = nearestNodeXZUnwalkable.CoordinatesInGrid;
		Vector2Int coordinatesInGrid2 = customGridNode.CoordinatesInGrid;
		if (coordinatesInGrid == coordinatesInGrid2)
		{
			return false;
		}
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(customGridNode, unit.SizeRect))
		{
			if (!dictionary.ContainsKey(node2.CoordinatesInGrid))
			{
				return false;
			}
		}
		if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(unit, customGridNode))
		{
			return false;
		}
		if (!ignorePetRestriction && unit.IsPet && unit.Master.DistanceToInCells(node.Vector3Position) > 2)
		{
			return false;
		}
		UnitPartPetOwner petOwner = unit.GetOptional<UnitPartPetOwner>();
		if (petOwner != null && !petOwner.PetUnit.HasMechanicFeature(MechanicsFeatureType.Hidden))
		{
			return dictionary.Any((KeyValuePair<Vector2Int, GraphNode> n) => unit.DistanceToInCells(node.Vector3Position, n.Value.Vector3Position, petOwner.PetUnit.SizeRect, unit.Forward) <= 2);
		}
		return true;
	}
}
