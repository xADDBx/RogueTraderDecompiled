using System;
using JetBrains.Annotations;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Controllers.Timer;

public class SimpleTimer : ITimer
{
	private readonly Action m_Callback;

	[CanBeNull]
	private readonly MechanicEntity m_MechanicEntityData;

	[CanBeNull]
	private readonly BaseUnitEntity m_InteractingUnitData;

	private readonly TimeSpan m_DueTime;

	public SimpleTimer(Action callback, TimeSpan timerDuration)
	{
		m_Callback = callback;
		m_DueTime = Game.Instance.TimeController.GameTime + timerDuration;
		m_MechanicEntityData = MechanicEntityData.CurrentEntity;
		m_InteractingUnitData = ContextData<InteractingUnitData>.Current?.Unit;
	}

	public bool Tick()
	{
		if (Game.Instance.TimeController.GameTime < m_DueTime)
		{
			return false;
		}
		RunCallback();
		return true;
	}

	private void RunCallback()
	{
		using ((m_MechanicEntityData != null) ? ContextData<MechanicEntityData>.Request().Setup(m_MechanicEntityData) : null)
		{
			using ((m_InteractingUnitData != null) ? ContextData<InteractingUnitData>.Request().Setup(m_InteractingUnitData) : null)
			{
				m_Callback?.Invoke();
			}
		}
	}
}
