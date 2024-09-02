using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.PubSubSystem.Core;

public readonly struct EventBusLoopGuard<TEvent> : IDisposable
{
	private static readonly Dictionary<object, int> Requested = new Dictionary<object, int>();

	[CanBeNull]
	private readonly object m_Subscriber;

	public bool Blocked => Requested.Get(m_Subscriber, 0) > 1;

	public static EventBusLoopGuard<TEvent> Request([CanBeNull] object subscriber)
	{
		if (!(subscriber is IEventBusLoopGuard))
		{
			return new EventBusLoopGuard<TEvent>(null);
		}
		Requested[subscriber] = Requested.Get(subscriber, 0) + 1;
		return new EventBusLoopGuard<TEvent>(subscriber);
	}

	private EventBusLoopGuard([CanBeNull] object subscriber)
	{
		m_Subscriber = subscriber;
	}

	public void Dispose()
	{
		if (m_Subscriber != null)
		{
			int num = Requested.Get(m_Subscriber, 0);
			if (num < 2)
			{
				Requested.Remove(m_Subscriber);
			}
			else
			{
				Requested[m_Subscriber] = num - 1;
			}
		}
	}
}
