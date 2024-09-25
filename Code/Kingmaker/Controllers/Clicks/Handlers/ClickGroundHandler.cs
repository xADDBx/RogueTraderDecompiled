using System.Collections.Generic;
using System.Linq;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Controllers.Units;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Networking;
using Kingmaker.Settings;
using Kingmaker.UI.Common;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.Random;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using UniRx;
using UnityEngine;

namespace Kingmaker.Controllers.Clicks.Handlers;

public class ClickGroundHandler : IClickEventHandler, IDragClickEventHandler
{
	private static Vector3 s_Direction;

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
		if (TurnController.IsInTurnBasedCombat() && Game.Instance.TurnController.IsPreparationTurn)
		{
			return 0f;
		}
		float result = ((KeyboardAccess.IsCtrlHold() || (Input.GetMouseButton(1) && (MouseRightButtonFunction)SettingsRoot.Controls.MouseRightButtonFunction == MouseRightButtonFunction.ForceMove && !Game.Instance.Player.UISettings.ShowInspect)) ? 100f : 0.1f);
		if (Game.Instance.ClickEventsController.IgnoreUnitsColliders)
		{
			result = 100f;
		}
		if (gameObject == null)
		{
			return result;
		}
		BaseUnitEntity baseUnitEntity = gameObject.GetComponentNonAlloc<UnitEntityView>()?.EntityData;
		if (baseUnitEntity != null && !baseUnitEntity.IsConscious)
		{
			return result;
		}
		if (!gameObject.IsLayerMask(Layers.WalkableMask))
		{
			return 0f;
		}
		return result;
	}

	public bool OnClick(GameObject gameObject, Vector3 worldPosition, int button, bool simulate = false, bool muteEvents = false)
	{
		if (PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingPosition(worldPosition);
		}))
		{
			return false;
		}
		using (PFStatefulRandom.StartUiContext())
		{
			UISounds.Instance.Sounds.Combat.NotInCombatSetWaypointClick.Play();
			switch (button)
			{
			case 1:
				if (Game.Instance.Player.IsInCombat)
				{
					if (!Game.Instance.TurnController.CurrentUnit.IsDirectlyControllable())
					{
						return false;
					}
					UnitCommandsRunner.CancelMoveCommand();
				}
				return false;
			case 0:
				UnitCommandsRunner.MoveSelectedUnitsToPoint(worldPosition);
				return true;
			default:
				return false;
			}
		}
	}

	public bool OnClick(GameObject gameObject, Vector3 startDrag, Vector3 endDrag)
	{
		UISounds.Instance.Sounds.Combat.NotInCombatSetWaypointClick.Play();
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(startDrag, s_Direction, Game.Instance.IsControllerGamepad);
		return true;
	}

	public bool OnDrag(GameObject gameObject, Vector3 startDrag, Vector3 endDrag)
	{
		Vector3 vector = endDrag - startDrag;
		if (vector.magnitude > 0.2f)
		{
			s_Direction = vector;
			if (!UnitCommandsRunner.HasWaitingAgents)
			{
				UnitCommandsRunner.MoveSelectedUnitsToPointRT(startDrag, s_Direction, isControllerGamepad: false, preview: true);
			}
			else
			{
				UnitCommandsRunner.ClearWaitingAgents(delayed: true);
			}
		}
		return true;
	}

	public bool OnStartDrag(GameObject gameObject, Vector3 startDrag)
	{
		UnitCommandsRunner.ClearWaitingAgents();
		s_Direction = GetDefaultDirection(startDrag);
		UnitCommandsRunner.MoveSelectedUnitsToPointRT(startDrag, s_Direction, isControllerGamepad: false, preview: true);
		return true;
	}

	public static Vector3 GetDefaultDirection(Vector3 worldPosition)
	{
		ReactiveCollection<BaseUnitEntity> selectedUnits = Game.Instance.SelectionCharacter.SelectedUnits;
		return GetDirection(worldPosition, selectedUnits);
	}

	public static Vector3 GetDirection(Vector3 worldPosition, IList<BaseUnitEntity> units)
	{
		if (units.Count == 0)
		{
			return Vector3.zero;
		}
		Vector3 vector = units.Aggregate(Vector3.zero, (Vector3 current, BaseUnitEntity unit) => current + unit.Position);
		vector /= (float)units.Count;
		return worldPosition - vector;
	}
}
