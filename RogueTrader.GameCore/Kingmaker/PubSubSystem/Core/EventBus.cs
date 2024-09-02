using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public class EventBus
{
	public static bool DebugSubscriptions = false;

	[NotNull]
	public static readonly SubscriptionManager<ISubscriber> GlobalSubscribers = new SubscriptionManager<ISubscriber>();

	[NotNull]
	public static readonly PooledDictionary<IEntity, SubscriptionManager<ISubscriber>> EntitySubscribers = new PooledDictionary<IEntity, SubscriptionManager<ISubscriber>>();

	public static void LogActiveSubscribers()
	{
		PFLog.EventSystemDebug.Log("====> GLOBAL");
		GlobalSubscribers.LogActiveSubscribers();
		foreach (var (arg, subscriptionManager2) in EntitySubscribers)
		{
			PFLog.EventSystemDebug.Log($"====> {arg}");
			subscriptionManager2.LogActiveSubscribers();
		}
	}

	public static IDisposable Subscribe([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			Subscribe(subscriptionProxy.GetSubscriber(), subscriptionProxy);
		}
		Subscribe(subscriber as ISubscriber, null);
		RulebookEventBus.Subscribe(subscriber);
		return new EventBusSubscription(subscriber);
	}

	public static void Unsubscribe([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			Unsubscribe(subscriptionProxy.GetSubscriber(), subscriptionProxy);
		}
		Unsubscribe(subscriber as ISubscriber, null);
		RulebookEventBus.Unsubscribe(subscriber);
	}

	private static void Subscribe([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		SubscribeGlobal(subscriber, proxy);
		SubscribeBinded(subscriber, proxy);
	}

	private static void Unsubscribe([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		UnsubscribeGlobal(subscriber, proxy);
		UnsubscribeBinded(subscriber, proxy);
	}

	private static void SubscribeGlobal([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalSubscribers.Subscribe<EventTagNone>(subscriber, proxy);
		}
	}

	private static void UnsubscribeGlobal([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalSubscribers.Unsubscribe<EventTagNone>(subscriber, proxy);
		}
	}

	private static void SubscribeBinded([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			IEntity entity = proxy?.GetSubscribingEntity() ?? (subscriber as IEntitySubscriber)?.GetSubscribingEntity();
			if (entity != null)
			{
				EntitySubscribers.Sure(entity).Subscribe<EntitySubscriber>(subscriber, proxy);
			}
		}
	}

	private static void UnsubscribeBinded([CanBeNull] ISubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		IEntity entity = proxy?.GetSubscribingEntity() ?? (subscriber as IEntitySubscriber)?.GetSubscribingEntity();
		if (entity == null)
		{
			return;
		}
		SubscriptionManager<ISubscriber> subscriptionManager = EntitySubscribers.Get(entity);
		if (subscriptionManager != null)
		{
			subscriptionManager.Unsubscribe<EntitySubscriber>(subscriber, proxy);
			if (subscriptionManager.Empty())
			{
				EntitySubscribers.Remove(entity);
			}
		}
	}

	public static void ClearEntitySubscriptions([NotNull] IEntity entity)
	{
		EntitySubscribers.Remove(entity);
		RulebookEventBus.ClearEntitySubscriptions(entity);
	}

	public static bool IsGloballySubscribed(object subscriber)
	{
		if (!(subscriber is ISubscriber subscriber2))
		{
			return false;
		}
		return GlobalSubscribers.Contains(subscriber2);
	}

	public static void RaiseEvent<T>(Action<T> action, bool isCheckRuntime = true) where T : ISubscriber
	{
		try
		{
			GlobalSubscribers.RaiseEvent(action, isCheckRuntime);
		}
		finally
		{
		}
	}

	public static void RaiseEvent<T>(IEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IEntity>
	{
		EventBus.RaiseEvent<T, IEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IMechanicEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMechanicEntity>
	{
		EventBus.RaiseEvent<T, IMechanicEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IAbstractUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAbstractUnitEntity>
	{
		EventBus.RaiseEvent<T, IAbstractUnitEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IBaseUnitEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IBaseUnitEntity>
	{
		EventBus.RaiseEvent<T, IBaseUnitEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IAreaEffectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IAreaEffectEntity>
	{
		EventBus.RaiseEvent<T, IAreaEffectEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IMapObjectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IMapObjectEntity>
	{
		EventBus.RaiseEvent<T, IMapObjectEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(ISectorMapObjectEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<ISectorMapObjectEntity>
	{
		EventBus.RaiseEvent<T, ISectorMapObjectEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(ISectorMapPassageEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<ISectorMapPassageEntity>
	{
		EventBus.RaiseEvent<T, ISectorMapPassageEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IStarshipEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IStarshipEntity>
	{
		EventBus.RaiseEvent<T, IStarshipEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T>(IItemEntity entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<IItemEntity>
	{
		EventBus.RaiseEvent<T, IItemEntity>(entity, action, isCheckRuntime);
	}

	public static void RaiseEvent<T, TInvoker>(TInvoker entity, Action<T> action, bool isCheckRuntime = true) where T : ISubscriber<TInvoker> where TInvoker : IEntity
	{
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		try
		{
			using (ContextData<EventInvoker>.Request().Setup(entity))
			{
				RaiseEvent(action, isCheckRuntime);
			}
			EntitySubscribers.Get(entity)?.RaiseEvent(action);
		}
		finally
		{
		}
	}
}
