using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using UnityEngine;

namespace Kingmaker.Controllers.TurnBased;

public class VirtualPositionController : IController, ITurnStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IInterruptTurnStartHandler, ITurnEndHandler, IInterruptTurnEndHandler
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

	public VirtualPositionController(TurnController controller)
	{
		m_TurnController = controller;
	}

	public Vector3 GetDesiredPosition(MechanicEntity entity)
	{
		if (entity != m_TurnController.CurrentUnit)
		{
			return entity.Position;
		}
		if (!m_TurnController.CurrentUnit.IsInPlayerParty)
		{
			return entity.Position;
		}
		if (entity.GetCommandsOptional()?.Current != null)
		{
			return entity.Position;
		}
		return m_VirtualPosition ?? entity.Position;
	}

	public bool TryGetVirtualPosition(MechanicEntity entity, out Vector3 virtualPosition)
	{
		virtualPosition = Vector3.zero;
		if (entity != m_TurnController.CurrentUnit)
		{
			return false;
		}
		MechanicEntity currentUnit = m_TurnController.CurrentUnit;
		if (currentUnit != null && !currentUnit.IsInPlayerParty)
		{
			return false;
		}
		if (entity.GetCommandsOptional()?.Current != null)
		{
			return false;
		}
		if (!m_VirtualPosition.HasValue)
		{
			return false;
		}
		virtualPosition = m_VirtualPosition.Value;
		return true;
	}

	public Vector3 GetDesiredRotation(MechanicEntity entity)
	{
		if (entity != m_TurnController.CurrentUnit)
		{
			return entity.Forward;
		}
		if (!m_TurnController.CurrentUnit.IsInPlayerParty)
		{
			return entity.Forward;
		}
		if (entity.GetCommandsOptional()?.Current != null)
		{
			return entity.Forward;
		}
		return m_VirtualRotation ?? entity.Forward;
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

	void IInterruptTurnStartHandler.HandleUnitStartInterruptTurn()
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
}
