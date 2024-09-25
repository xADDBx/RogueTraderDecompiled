using System;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Sound.Base;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.View;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Controllers.Units;

public class UnitMoveController : IControllerEnable, IController, IControllerTick
{
	private class MoveHandlerWrapper
	{
		public AbstractUnitEntity Unit;

		public Action<IUnitMoveHandler> Handler { get; }

		public MoveHandlerWrapper()
		{
			Handler = delegate(IUnitMoveHandler h)
			{
				h.HandleUnitMovement(Unit);
			};
		}
	}

	private static bool s_IsStartRotate;

	private readonly MoveHandlerWrapper m_MoveHandlerWrapper = new MoveHandlerWrapper();

	void IControllerEnable.OnEnable()
	{
		foreach (AbstractUnitEntity allUnit in Game.Instance.State.AllUnits)
		{
			if (!(allUnit.View == null))
			{
				UnitMovementAgentBase movementAgent = allUnit.View.MovementAgent;
				if (!(movementAgent == null))
				{
					ObstaclesHelper.ConnectToGroups(movementAgent);
				}
			}
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		if (!Game.Instance.CurrentlyLoadedArea.IsNavmeshArea || ((bool)AstarPath.active && AstarPath.active.graphs.Length != 0))
		{
			float gameDeltaTime = Game.Instance.TimeController.GameDeltaTime;
			MovableEntitiesEnumerable.Enumerator enumerator = default(MovableEntitiesEnumerable).GetEnumerator();
			while (enumerator.MoveNext())
			{
				AbstractUnitEntity current = enumerator.Current;
				TickUnit(current, gameDeltaTime);
			}
		}
	}

	private void TickUnit([NotNull] AbstractUnitEntity unit, float dt)
	{
		UnitMovementAgentBase unitMovementAgentBase = unit.View?.MovementAgent;
		if (!(unitMovementAgentBase == null))
		{
			bool isReallyMoving = ApplyChangesByAgent(unit, unitMovementAgentBase, dt);
			TryUpdateTransformPosition(unit, isReallyMoving);
			TryUpdateTransformOrientation(unit);
			LogTransform(unit);
			UpdateAgentVelocity(unitMovementAgentBase);
		}
	}

	private bool ApplyChangesByAgent(AbstractUnitEntity unit, UnitMovementAgentBase agent, float dt)
	{
		bool flag = agent.IsReallyMoving && unit.CanMove;
		bool canRotate = unit.CanRotate;
		if (flag)
		{
			agent.TickMovement(dt);
			unit.Movable.LastMoveTime = Game.Instance.TimeController.GameTime;
			if (unit.Movable.HasMotionThisSimulationTick && canRotate && agent.IsReallyMoving && !agent.NodeLinkTraverser.IsTraverseNow)
			{
				Vector3 forward = agent.MoveDirection.To3D();
				if (forward.sqrMagnitude > 0.01f)
				{
					unit.SetOrientation(Quaternion.LookRotation(forward).eulerAngles.y);
				}
			}
			using (ProfileScope.New("Unit move handlers"))
			{
				m_MoveHandlerWrapper.Unit = unit;
				EventBus.RaiseEvent((IAbstractUnitEntity)unit, m_MoveHandlerWrapper.Handler, isCheckRuntime: true);
				m_MoveHandlerWrapper.Unit = null;
			}
		}
		if (canRotate)
		{
			float num = ((Game.Instance.CurrentMode == GameModeType.Dialog) ? (Game.Instance.TimeController.DeltaTime * agent.m_AngularSpeed) : (dt * agent.m_AngularSpeed * unit.Movable.SlowMoSpeedMod));
			num *= Game.CombatAnimSpeedUp;
			unit.UpdateSlowRotation(num);
		}
		return flag;
	}

	private static void TryUpdateTransformPosition(AbstractUnitEntity unit, bool isReallyMoving)
	{
		AbstractUnitEntityView view = unit.View;
		float num = 0f;
		if (unit.Movable.PreviousSimulationTick.HasMotion)
		{
			Vector3 viewPosition = view.InterpolationHelper.GetViewPosition(unit.Movable.PreviousPosition);
			Transform transform = view.transform;
			num = (transform.position - viewPosition).sqrMagnitude;
			transform.position = viewPosition;
		}
		if (!isReallyMoving)
		{
			if (unit.Movable.PreviousSimulationTick.HasMotion || (view.AnimationManager != null && view.AnimationManager.IsGoingProne) || (bool)unit.Features.OnElevator)
			{
				view.ForcePlaceAboveGround();
			}
			if (num >= 1f)
			{
				view.IkController?.GrounderIk?.ResetPosition();
			}
		}
	}

	private static void TryUpdateTransformOrientation(AbstractUnitEntity unit)
	{
		if ((Mathf.Approximately(unit.Orientation, unit.Movable.PreviousOrientation) && !unit.Movable.PreviousSimulationTick.HasRotation) || unit.View.ForbidRotation)
		{
			return;
		}
		if (!unit.View.OverrideRotatablePart)
		{
			unit.View.transform.rotation = Quaternion.Euler(0f, unit.Movable.PreviousOrientation, 0f);
		}
		else
		{
			unit.View.OverrideRotatablePart.transform.rotation = Quaternion.Euler(0f, unit.Movable.PreviousOrientation, 0f);
		}
		if ((bool)unit.View.OverrideRotatablePart && !s_IsStartRotate)
		{
			if (unit.View.gameObject != null && unit.Blueprint.VisualSettings?.TurettRotateStart != null)
			{
				SoundEventsManager.PostEvent(unit.Blueprint.VisualSettings.TurettRotateStart, unit.View.gameObject);
				s_IsStartRotate = true;
			}
			else
			{
				PFLog.TechArt.Warning("unit.View.gameObject or unit.Blueprint.VisualSettings.TurettRotateStart is null");
			}
		}
		if ((bool)unit.View.OverrideRotatablePart && Mathf.Approximately(unit.View.OverrideRotatablePart.transform.rotation.eulerAngles.y, unit.DesiredOrientation))
		{
			if (unit.View.gameObject != null && unit.Blueprint.VisualSettings?.TurettRotateStop != null)
			{
				SoundEventsManager.PostEvent(unit.Blueprint.VisualSettings.TurettRotateStop, unit.View.gameObject);
				s_IsStartRotate = false;
			}
			else
			{
				PFLog.TechArt.Warning("unit.View.gameObject or unit.Blueprint.VisualSettings.TurettRotateStop is null");
			}
		}
	}

	private static void UpdateAgentVelocity(UnitMovementAgentBase agent)
	{
		if (!Game.Instance.IsPaused)
		{
			agent.UpdateVelocity();
		}
	}

	private static void LogTransform(AbstractUnitEntity unit)
	{
	}
}
