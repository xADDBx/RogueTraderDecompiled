using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class VirtualPositionController : IController, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IContinueTurnHandler, IInterruptTurnStartHandler, ITurnEndHandler, IInterruptTurnEndHandler, IInterruptTurnContinueHandler
{
	private readonly TurnController m_TurnController;

	private Vector3? m_VirtualPosition;

	private Vector3? m_VirtualRotation;

	public bool HasVirtualPosition => m_VirtualPosition.HasValue;

	public Vector3? VirtualPosition
	{
		get
		{
			return m_VirtualPosition;
		}
		set
		{
			if (!(m_VirtualPosition == value))
			{
				m_VirtualPosition = value;
				EventBus.RaiseEvent(delegate(IVirtualPositionUIHandler h)
				{
					h.HandleVirtualPositionChanged(m_VirtualPosition);
				});
			}
		}
	}

	public Vector3? VirtualRotation
	{
		get
		{
			return m_VirtualRotation;
		}
		set
		{
			if (!(m_VirtualRotation == value))
			{
				m_VirtualRotation = value;
			}
		}
	}

	public MechanicEntity VirtualPositionUnit { get; set; }

	public MechanicEntity CurrentUnit => VirtualPositionUnit ?? m_TurnController.CurrentUnit;

	public VirtualPositionController(TurnController controller)
	{
		m_TurnController = controller;
	}

	public Vector3 GetDesiredPosition(MechanicEntity entity)
	{
		if (!TryGetVirtualPosition(entity, out var virtualPosition))
		{
			return entity.Position;
		}
		return virtualPosition;
	}

	public bool TryGetVirtualPosition(MechanicEntity entity, out Vector3 virtualPosition)
	{
		if (entity != CurrentUnit || (entity != null && !entity.IsInPlayerParty) || entity.GetCommandsOptional()?.Current != null || !m_VirtualPosition.HasValue)
		{
			virtualPosition = Vector3.zero;
			return false;
		}
		virtualPosition = m_VirtualPosition.Value;
		return true;
	}

	public Vector3 GetDesiredRotation(MechanicEntity entity)
	{
		if (entity != CurrentUnit || !entity.IsInPlayerParty || entity.GetCommandsOptional()?.Current != null || !m_VirtualRotation.HasValue)
		{
			return entity.Forward;
		}
		return m_VirtualRotation.Value;
	}

	public void CleanVirtualPosition()
	{
		VirtualPosition = null;
		VirtualRotation = null;
	}

	void ITurnStartHandler.HandleUnitStartTurn(bool isTurnBased)
	{
		CleanVirtualPosition();
	}

	void IContinueTurnHandler.HandleUnitContinueTurn(bool isTurnBased)
	{
		CleanVirtualPosition();
	}

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn(InterruptionData interruptionData)
	{
		CleanVirtualPosition();
	}

	void ITurnEndHandler.HandleUnitEndTurn(bool isTurnBased)
	{
		CleanVirtualPosition();
	}

	void IInterruptTurnEndHandler.HandleUnitEndInterruptTurn()
	{
		CleanVirtualPosition();
	}

	void IInterruptTurnContinueHandler.HandleUnitContinueInterruptTurn()
	{
		CleanVirtualPosition();
	}
}
