using System;

namespace Kingmaker.PubSubSystem.Core;

public class EventBusSubscription : IDisposable
{
	private readonly object m_Subscriber;

	public EventBusSubscription(object subscriber)
	{
		m_Subscriber = subscriber;
	}

	public void Dispose()
	{
		EventBus.Unsubscribe(m_Subscriber);
	}
}
