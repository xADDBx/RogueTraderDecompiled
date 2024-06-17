using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.QA;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UI.Models.Log.Events;

public abstract class GameLogEvent
{
	[CanBeNull]
	private List<GameLogEvent> m_InnerEvents;

	[CanBeNull]
	public GameLogEvent ParentEvent { get; private set; }

	public ReadonlyList<GameLogEvent> InnerEvents => m_InnerEvents;

	public virtual bool IsEnabled => !ContextData<GameLogDisabled>.Current;

	public virtual bool IsReady => true;

	public bool TryAddInnerEvent(GameLogEvent @event)
	{
		try
		{
			if (m_InnerEvents.HasItem(@event))
			{
				return true;
			}
			bool flag = TryHandleInnerEventInternal(@event);
			if (flag)
			{
				(m_InnerEvents ?? (m_InnerEvents = new List<GameLogEvent>())).Add(@event);
				@event.ParentEvent = this;
			}
			for (GameLogEvent parentEvent = ParentEvent; parentEvent != null; parentEvent = parentEvent.ParentEvent)
			{
				bool flag2 = TryHandleInnerEventInternal(@event);
				if (!flag && flag2)
				{
					flag = true;
					GameLogEvent gameLogEvent = parentEvent;
					(gameLogEvent.m_InnerEvents ?? (gameLogEvent.m_InnerEvents = new List<GameLogEvent>())).Add(@event);
					@event.ParentEvent = parentEvent;
				}
			}
			return flag;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
	}

	protected virtual bool TryHandleInnerEventInternal(GameLogEvent @event)
	{
		return true;
	}

	public bool TrySwallowEvent(GameLogEvent @event)
	{
		try
		{
			if (@event.ParentEvent != null)
			{
				PFLog.Default.ErrorWithReport("GameLogEvent: attempt to swallow event which ParentEvent != null");
				return false;
			}
			bool num = TrySwallowEventInternal(@event);
			if (num && !TryAddInnerEvent(@event))
			{
				PFLog.Default.ErrorWithReport("GameLogEvent: failed to merge events");
			}
			return num;
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return false;
		}
	}

	protected virtual bool TrySwallowEventInternal(GameLogEvent @event)
	{
		return false;
	}

	public abstract void Invoke(LogThreadBase logThread);
}
public abstract class GameLogEvent<TSelf> : GameLogEvent where TSelf : GameLogEvent<TSelf>
{
	public sealed override void Invoke(LogThreadBase logThread)
	{
		(logThread as IGameLogEventHandler<TSelf>)?.HandleEvent((TSelf)this);
	}
}
