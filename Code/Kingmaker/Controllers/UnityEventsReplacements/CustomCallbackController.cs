using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Controllers.UnityEventsReplacements;

public class CustomCallbackController : IControllerTick, IController, IControllerStop
{
	public class ActionWrapper
	{
		public uint id;

		public Action callback;

		public TimeSpan time;

		public void Reset()
		{
			id = 0u;
			callback = null;
			time = default(TimeSpan);
		}
	}

	private readonly List<ActionWrapper> m_Callbacks = new List<ActionWrapper>();

	private readonly Stack<ActionWrapper> m_Cache = new Stack<ActionWrapper>();

	private uint m_LastId;

	TickType IControllerTick.GetTickType()
	{
		return TickType.Simulation;
	}

	void IControllerTick.Tick()
	{
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		while (0 < m_Callbacks.Count)
		{
			ActionWrapper actionWrapper = m_Callbacks[0];
			if (!(gameTime < actionWrapper.time))
			{
				m_Callbacks.RemoveAt(0);
				actionWrapper.callback();
				ReleaseActionWrapper(actionWrapper);
				continue;
			}
			break;
		}
	}

	void IControllerStop.OnStop()
	{
		int i = 0;
		for (int count = m_Callbacks.Count; i < count; i++)
		{
			ReleaseActionWrapper(m_Callbacks[i]);
		}
		m_Callbacks.Clear();
	}

	public uint InvokeInTime([NotNull] Action callback, float delayInSeconds)
	{
		m_LastId = m_LastId % uint.MaxValue + 1;
		ActionWrapper actionWrapper = RetainActionWrapper();
		actionWrapper.id = m_LastId;
		actionWrapper.callback = callback;
		actionWrapper.time = Game.Instance.TimeController.GameTime + delayInSeconds.Seconds();
		bool flag = false;
		int i = 0;
		for (int count = m_Callbacks.Count; i < count; i++)
		{
			if (actionWrapper.time < m_Callbacks[i].time)
			{
				m_Callbacks.Insert(i, actionWrapper);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_Callbacks.Add(actionWrapper);
		}
		return actionWrapper.id;
	}

	public bool Cancel(uint actionId)
	{
		int i = 0;
		for (int count = m_Callbacks.Count; i < count; i++)
		{
			ActionWrapper actionWrapper = m_Callbacks[i];
			if (actionWrapper.id == actionId)
			{
				m_Callbacks.RemoveAt(i);
				ReleaseActionWrapper(actionWrapper);
				return true;
			}
		}
		return false;
	}

	private ActionWrapper RetainActionWrapper()
	{
		if (!m_Cache.TryPop(out var result))
		{
			return new ActionWrapper();
		}
		return result;
	}

	private void ReleaseActionWrapper(ActionWrapper wrapper)
	{
		wrapper.Reset();
		m_Cache.Push(wrapper);
	}
}
