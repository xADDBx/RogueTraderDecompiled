using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding.Util;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookSubscriptionManager<TSubscriber> : IAstarPooledObject where TSubscriber : class
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

	private readonly Dictionary<Type, IRulebookSubscribersList> m_Listeners = new Dictionary<Type, IRulebookSubscribersList>();

	public void Subscribe(TSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		List<Type> subscribedRulebookTypes = InterfaceFinder<TSubscriber>.GetSubscribedRulebookTypes(subscriber);
		object subscriber2 = ((object)proxy) ?? ((object)subscriber);
		foreach (Type item in subscribedRulebookTypes)
		{
			if (!m_Listeners.TryGetValue(item, out var value))
			{
				ConstructorInfo constructor = typeof(RulebookSubscribersList<>).MakeGenericType(item).GetConstructor(Type.EmptyTypes);
				if (constructor == null)
				{
					PFLog.Default.Error("Cannot find empty constructor");
					return;
				}
				value = constructor.Invoke(null) as IRulebookSubscribersList;
				if (value == null)
				{
					PFLog.Default.Error("Failed to create subscribers list");
					return;
				}
				m_Listeners[item] = value;
			}
			value.AddSubscriber(subscriber2);
		}
		if (RulebookEventBus.DebugSubscriptions && !DebugStore.ContainsKey(subscriber.GetHashCode()))
		{
			DebugStore.Add(subscriber.GetHashCode(), new DebugEntry(new StackTrace(2), subscriber.ToString(), subscriber.GetType().FullName));
		}
	}

	public void Unsubscribe(TSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		List<Type> subscribedRulebookTypes = InterfaceFinder<TSubscriber>.GetSubscribedRulebookTypes(subscriber);
		object subscriber2 = ((object)proxy) ?? ((object)subscriber);
		foreach (Type item in subscribedRulebookTypes)
		{
			if (m_Listeners.TryGetValue(item, out var value))
			{
				value.RemoveSubscriber(subscriber2);
				if (value.Empty())
				{
					m_Listeners.Remove(item);
				}
			}
		}
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

	public void OnEventAboutToTrigger(IRulebookEvent evt)
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			Type type = evt.GetType();
			while (type != null && type != evt.RootType)
			{
				if (m_Listeners.TryGetValue(type, out var value))
				{
					value.OnEventAboutToTrigger(evt);
				}
				type = type.BaseType;
			}
		}
	}

	public void OnEventDidTrigger(IRulebookEvent evt)
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			Type type = evt.GetType();
			while (type != null && type != evt.RootType)
			{
				if (m_Listeners.TryGetValue(type, out var value))
				{
					value.OnEventDidTrigger(evt);
				}
				type = type.BaseType;
			}
		}
	}

	public bool Empty()
	{
		return m_Listeners.Count <= 0;
	}

	public void OnEnterPool()
	{
		m_Listeners.Clear();
	}
}
