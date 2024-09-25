using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookSubscribersList<TEvent> : SubscribersList<IRulebookHandler<TEvent>>, IRulebookSubscribersList where TEvent : IRulebookEvent
{
	public bool Empty()
	{
		return List.Count == 0;
	}

	void IRulebookSubscribersList.AddSubscriber(object subscriber)
	{
		AddSubscriber(subscriber);
	}

	void IRulebookSubscribersList.RemoveSubscriber(object subscriber)
	{
		RemoveSubscriber(subscriber);
	}

	public void OnEventAboutToTrigger(IRulebookEvent evt)
	{
		bool executing = Executing;
		Executing = true;
		try
		{
			List<object> list = List.ToTempList();
			for (int i = 0; i < list.Count; i++)
			{
				object obj = list[i];
				if (obj != null)
				{
					try
					{
						OnEventAboutToTrigger((TEvent)evt, obj);
					}
					catch (Exception ex)
					{
						PFLog.Default.Exception(ex);
					}
				}
			}
		}
		finally
		{
			Executing = executing;
			if (!Executing)
			{
				Cleanup();
			}
			Cleanup();
		}
	}

	public void OnEventDidTrigger(IRulebookEvent evt)
	{
		bool executing = Executing;
		Executing = true;
		try
		{
			List<object> list = List.ToTempList();
			for (int i = 0; i < list.Count; i++)
			{
				object obj = list[i];
				if (obj != null)
				{
					try
					{
						OnEventDidTrigger((TEvent)evt, obj);
					}
					catch (Exception ex)
					{
						PFLog.Default.Exception(ex);
					}
				}
			}
		}
		finally
		{
			Executing = executing;
			if (!Executing)
			{
				Cleanup();
			}
		}
	}

	private static void OnEventAboutToTrigger(TEvent evt, object handler)
	{
		if (handler is ISubscriptionProxy subscriptionProxy)
		{
			using (subscriptionProxy.RequestEventContext())
			{
				ExecuteOnEventAboutToTrigger(evt, subscriptionProxy.GetSubscriber(), handler);
			}
		}
		ExecuteOnEventAboutToTrigger(evt, handler);
	}

	private static void OnEventDidTrigger(TEvent evt, object handler)
	{
		if (handler is ISubscriptionProxy subscriptionProxy)
		{
			using (subscriptionProxy.RequestEventContext())
			{
				ExecuteOnEventDidTrigger(evt, subscriptionProxy.GetSubscriber(), handler);
			}
		}
		ExecuteOnEventDidTrigger(evt, handler);
	}

	private static void ExecuteOnEventAboutToTrigger(TEvent evt, object eventTarget, object eventSubscriber = null)
	{
		if (!(eventTarget is IRulebookHandler<TEvent> rulebookHandler))
		{
			return;
		}
		if (eventSubscriber == null)
		{
			eventSubscriber = eventTarget;
		}
		using EventBusLoopGuard<TEvent> eventBusLoopGuard = EventBusLoopGuard<TEvent>.Request(eventSubscriber);
		if (!eventBusLoopGuard.Blocked)
		{
			rulebookHandler.OnEventAboutToTrigger(evt);
		}
	}

	private static void ExecuteOnEventDidTrigger(TEvent evt, object eventTarget, object eventSubscriber = null)
	{
		if (!(eventTarget is IRulebookHandler<TEvent> rulebookHandler))
		{
			return;
		}
		if (eventSubscriber == null)
		{
			eventSubscriber = eventTarget;
		}
		using EventBusLoopGuard<TEvent> eventBusLoopGuard = EventBusLoopGuard<TEvent>.Request(eventSubscriber);
		if (!eventBusLoopGuard.Blocked)
		{
			rulebookHandler.OnEventDidTrigger(evt);
		}
	}
}
