using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.ElementsSystem.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding.Util;

namespace Kingmaker.PubSubSystem.Core;

public class SubscriptionManager<TSubscriber> : IAstarPooledObject where TSubscriber : class
{
	public readonly struct DebugEntry
	{
		public readonly StackTrace CreatedCallstack;

		public readonly string ObjectName;

		public readonly string ObjectType;

		public DebugEntry(StackTrace createdCallstack, string objectName, string objectType)
		{
			CreatedCallstack = createdCallstack;
			ObjectName = objectName;
			ObjectType = objectType;
		}
	}

	public readonly Dictionary<int, DebugEntry> DebugStore = new Dictionary<int, DebugEntry>();

	public readonly Dictionary<int, DebugEntry> DebugStorePrev = new Dictionary<int, DebugEntry>();

	private readonly PooledDictionary<Type, SubscribersList<TSubscriber>> m_Listeners = new PooledDictionary<Type, SubscribersList<TSubscriber>>();

	public void Subscribe<TTag>(TSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		IEnumerable<Type> subscriberInterfaces = InterfaceFinder<TSubscriber>.GetSubscriberInterfaces<TTag>(subscriber);
		SubscribeInterfaces(subscriber, proxy, subscriberInterfaces);
		if (EventBus.DebugSubscriptions)
		{
			_ = subscriber.GetType().Name;
			if (!DebugStore.ContainsKey(subscriber.GetHashCode()))
			{
				DebugStore.Add(subscriber.GetHashCode(), new DebugEntry(new StackTrace(2), subscriber.ToString(), subscriber.GetType().FullName));
			}
		}
	}

	private void SubscribeInterfaces(TSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy, IEnumerable<Type> interfaces)
	{
		if (subscriber == null)
		{
			PFLog.Default.Error("Попытка подписать null на что-либо");
			return;
		}
		object obj = ((object)proxy) ?? ((object)subscriber);
		foreach (Type @interface in interfaces)
		{
			SubscribersList<TSubscriber> subscribersList = m_Listeners.Sure(@interface);
			if (subscribersList.List.Contains(obj) && (!(subscriber is ISubscriptionProxy subscriptionProxy) || !(subscriptionProxy.GetSubscriber() is TSubscriber)))
			{
				PFLog.Default.Error("Can't subscribe on events twice ({0}: {1})", obj.GetType().Name, @interface.Name);
			}
			else
			{
				subscribersList.List.Add(obj);
			}
		}
	}

	public void RaiseEvent<T>(Action<T> action, bool isCheckRuntime = true) where T : TSubscriber
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			try
			{
				SubscribersList<TSubscriber> subscribersList = m_Listeners.Get(typeof(T));
				if (subscribersList == null)
				{
					return;
				}
				bool executing = subscribersList.Executing;
				subscribersList.Executing = true;
				try
				{
					int count = subscribersList.List.Count;
					for (int i = 0; i < count; i++)
					{
						if (i >= subscribersList.List.Count || subscribersList.List[i] == null)
						{
							continue;
						}
						try
						{
							object obj = subscribersList.List[i];
							if (obj != null)
							{
								ExecuteAction(action, obj);
							}
							else
							{
								PFLog.Default.Error($"subscriber for {typeof(T)} is null");
							}
						}
						catch (Exception ex)
						{
							PFLog.Default.Exception(ex);
						}
						finally
						{
						}
					}
				}
				finally
				{
					subscribersList.Executing = executing;
					if (!subscribersList.Executing)
					{
						subscribersList.Cleanup();
					}
					if (subscribersList.List.Count <= 0)
					{
						m_Listeners.Remove(typeof(T));
					}
				}
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2);
			}
			finally
			{
			}
		}
	}

	private static void ExecuteAction<T>(Action<T> action, object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			using (subscriptionProxy.RequestEventContext())
			{
				ExecuteActionInternal(action, subscriptionProxy.GetSubscriber(), subscriber);
			}
		}
		ExecuteActionInternal(action, subscriber);
	}

	private static void ExecuteActionInternal<T>(Action<T> action, object eventTarget, object eventSubscriber = null)
	{
		if (!(eventTarget is T obj))
		{
			return;
		}
		if (eventSubscriber == null)
		{
			eventSubscriber = eventTarget;
		}
		using EventBusLoopGuard<T> eventBusLoopGuard = EventBusLoopGuard<T>.Request(eventSubscriber);
		if (eventBusLoopGuard.Blocked)
		{
			return;
		}
		if (eventTarget is IContextDataProvider contextDataProvider)
		{
			using (contextDataProvider.RequestContextData())
			{
				action(obj);
				return;
			}
		}
		action(obj);
	}

	public void Unsubscribe<TTag>(TSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		IEnumerable<Type> subscriberInterfaces = InterfaceFinder<TSubscriber>.GetSubscriberInterfaces<TTag>(subscriber);
		UnsubscribeInterfaces(subscriber, proxy, subscriberInterfaces);
		if (EventBus.DebugSubscriptions)
		{
			DebugStore.Remove(subscriber.GetHashCode());
		}
	}

	public void LogActiveSubscribers()
	{
		foreach (IGrouping<string, DebugEntry> item in from e in DebugStore.Values
			group e by e.ObjectType into g
			orderby g.Key
			select g)
		{
			PFLog.EventSystemDebug.Log($"\r\n============================================================================\r\n        {item.Key}: {item.Count()} total subscribers\r\n============================================================================");
			foreach (DebugEntry item2 in item)
			{
				PFLog.EventSystemDebug.Log($"{item2.ObjectName}\n{item2.CreatedCallstack}");
			}
		}
	}

	public void LogActiveSubscribersIncreased()
	{
		if (DebugStorePrev.Count > 0)
		{
			IEnumerable<IGrouping<string, DebugEntry>> enumerable = from e in DebugStore.Values
				group e by e.ObjectType;
			IEnumerable<IGrouping<string, DebugEntry>> source = from e in DebugStorePrev.Values
				group e by e.ObjectType;
			foreach (IGrouping<string, DebugEntry> group in enumerable)
			{
				int num = 0;
				if (source.TryFind((IGrouping<string, DebugEntry> g) => g.Key == group.Key, out var _))
				{
					num = source.Count();
					if (num >= group.Count())
					{
						continue;
					}
				}
				PFLog.EventSystemDebug.Log($"\r\n============================================================================\r\n        {group.Key}: {group.Count()} total subscribers (+{group.Count() - num})\r\n============================================================================");
				foreach (DebugEntry item in group)
				{
					PFLog.EventSystemDebug.Log($"{item.ObjectName}\n{item.CreatedCallstack}");
				}
			}
		}
		DebugStorePrev.Clear();
		foreach (var (key, value) in DebugStore)
		{
			DebugStorePrev.Add(key, value);
		}
	}

	private void UnsubscribeInterfaces(TSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy, IEnumerable<Type> interfaces)
	{
		if (subscriber == null)
		{
			return;
		}
		object subscriber2 = ((object)proxy) ?? ((object)subscriber);
		foreach (Type @interface in interfaces)
		{
			SubscribersList<TSubscriber> subscribersList = m_Listeners.Get(@interface);
			if (subscribersList != null)
			{
				subscribersList.RemoveSubscriber(subscriber2);
				if (subscribersList.List.Count <= 0)
				{
					m_Listeners.Remove(@interface);
				}
			}
		}
	}

	public bool Empty()
	{
		return m_Listeners.Count <= 0;
	}

	public bool Contains(TSubscriber subscriber)
	{
		foreach (SubscribersList<TSubscriber> value in m_Listeners.Values)
		{
			if (value.List.Contains(subscriber))
			{
				return true;
			}
		}
		return false;
	}

	public void OnEnterPool()
	{
		foreach (SubscribersList<TSubscriber> value in m_Listeners.Values)
		{
			SubscribersList<TSubscriber> obj = value;
			ObjectPool<SubscribersList<TSubscriber>>.Release(ref obj);
		}
		m_Listeners.Clear();
	}
}
