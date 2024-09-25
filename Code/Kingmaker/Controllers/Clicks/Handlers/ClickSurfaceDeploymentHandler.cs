using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Inspect;
using Kingmaker.Networking;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common;
using Kingmaker.UI.PathRenderer;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickSurfaceDeploymentHandler : IClickEventHandler
{
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
		if (unit == null || !unit.EntityData.IsDirectlyControllable)
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
		if (!CanDeployUnit(customGridNodeBase, value.SizeRect))
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(LocalizedTexts.Instance.Reasons.UnavailableGeneric, addToLog: false, WarningNotificationFormat.Attention);
			});
			return false;
		}
		UnitTeleportParams cmdParams = new UnitTeleportParams(customGridNodeBase.Vector3Position, isSynchronized: true);
		value.Commands.Run(cmdParams);
		return true;
	}

	public static bool CanDeployUnit(GraphNode node, IntRect sizeRect)
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
		BaseUnitEntity value = Game.Instance.SelectionCharacter.SelectedUnit.Value;
		if (!value.CanMove)
		{
			return false;
		}
		CustomGridNodeBase nearestNodeXZUnwalkable = value.Position.GetNearestNodeXZUnwalkable();
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
		foreach (CustomGridNodeBase node2 in GridAreaHelper.GetNodes(customGridNode, sizeRect))
		{
			if (!dictionary.ContainsKey(node2.CoordinatesInGrid))
			{
				return false;
			}
		}
		if (!WarhammerBlockManager.Instance.CanUnitStandOnNode(value, customGridNode))
		{
			return false;
		}
		return true;
	}
}
