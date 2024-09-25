using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookEventContext
{
	private readonly List<IRulebookEvent> m_EventStack = new List<IRulebookEvent>();

	private readonly IRulebookTrigger m_RulebookTrigger;

	[CanBeNull]
	public IRulebookEvent Current
	{
		get
		{
			if (m_EventStack.Count <= 0)
			{
				return null;
			}
			List<IRulebookEvent> eventStack = m_EventStack;
			return eventStack[eventStack.Count - 1];
		}
	}

	[CanBeNull]
	public IRulebookEvent Previous
	{
		get
		{
			if (m_EventStack.Count <= 1)
			{
				return null;
			}
			List<IRulebookEvent> eventStack = m_EventStack;
			return eventStack[eventStack.Count - 2];
		}
	}

	[CanBeNull]
	public IRulebookEvent First
	{
		get
		{
			if (m_EventStack.Count <= 0)
			{
				return null;
			}
			return m_EventStack[0];
		}
	}

	public RulebookEventContext(IRulebookTrigger rulebookTrigger)
	{
		m_RulebookTrigger = rulebookTrigger;
	}

	private RulebookEventContext(RulebookEventContext other)
	{
		m_EventStack = other.m_EventStack.ToList();
		m_RulebookTrigger = other.m_RulebookTrigger;
	}

	public RulebookEventContext Clone()
	{
		return new RulebookEventContext(this);
	}

	public void PushEvent(IRulebookEvent evt)
	{
		m_EventStack.Add(evt);
	}

	public void PopEvent(IRulebookEvent evt)
	{
		m_EventStack.RemoveAt(m_EventStack.Count - 1);
	}

	public TEvent Trigger<TEvent>(TEvent evt) where TEvent : IRulebookEvent
	{
		return m_RulebookTrigger.Trigger(this, evt);
	}

	[CanBeNull]
	public TEvent LastEventOfType<TEvent>() where TEvent : class, IRulebookEvent
	{
		for (int num = m_EventStack.Count - 1; num >= 0; num--)
		{
			if (m_EventStack[num] is TEvent result)
			{
				return result;
			}
		}
		return null;
	}

	[CanBeNull]
	public TEvent FirstEventOfType<TEvent>() where TEvent : class, IRulebookEvent
	{
		for (int i = 0; i < m_EventStack.Count; i++)
		{
			if (m_EventStack[i] is TEvent result)
			{
				return result;
			}
		}
		return null;
	}
}
