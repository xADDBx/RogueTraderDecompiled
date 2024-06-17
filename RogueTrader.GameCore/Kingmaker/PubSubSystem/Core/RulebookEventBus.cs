using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookEventBus
{
	public static bool DebugSubscriptions = false;

	[NotNull]
	private static readonly RulebookEventHooksManager Hooks = new RulebookEventHooksManager();

	[NotNull]
	public static readonly RulebookSubscriptionManager<IGlobalRulebookSubscriber> GlobalRulebookSubscribers = new RulebookSubscriptionManager<IGlobalRulebookSubscriber>();

	[NotNull]
	public static readonly PooledDictionary<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>> TargetRulebookSubscribers = new PooledDictionary<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>>();

	[NotNull]
	public static readonly PooledDictionary<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>> InitiatorRulebookSubscribers = new PooledDictionary<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>>();

	public static void LogActiveSubscribers()
	{
		PFLog.EventSystemDebug.Log("====> GLOBAL");
		GlobalRulebookSubscribers.LogActiveSubscribers();
		IEntity key;
		foreach (KeyValuePair<IEntity, RulebookSubscriptionManager<ITargetRulebookSubscriber>> targetRulebookSubscriber in TargetRulebookSubscribers)
		{
			targetRulebookSubscriber.Deconstruct(out key, out var value);
			IEntity arg = key;
			RulebookSubscriptionManager<ITargetRulebookSubscriber> rulebookSubscriptionManager = value;
			PFLog.EventSystemDebug.Log($"====> {arg}");
			rulebookSubscriptionManager.LogActiveSubscribers();
		}
		foreach (KeyValuePair<IEntity, RulebookSubscriptionManager<IInitiatorRulebookSubscriber>> initiatorRulebookSubscriber in InitiatorRulebookSubscribers)
		{
			initiatorRulebookSubscriber.Deconstruct(out key, out var value2);
			IEntity arg2 = key;
			RulebookSubscriptionManager<IInitiatorRulebookSubscriber> rulebookSubscriptionManager2 = value2;
			PFLog.EventSystemDebug.Log($"====> {arg2}");
			rulebookSubscriptionManager2.LogActiveSubscribers();
		}
	}

	internal static void Subscribe([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			Subscribe(subscriptionProxy.GetSubscriber() as IGlobalRulebookSubscriber, subscriptionProxy);
			Subscribe(subscriptionProxy.GetSubscriber() as ITargetRulebookSubscriber, subscriptionProxy);
			Subscribe(subscriptionProxy.GetSubscriber() as IInitiatorRulebookSubscriber, subscriptionProxy);
		}
		Subscribe(subscriber as IGlobalRulebookSubscriber, null);
		Subscribe(subscriber as ITargetRulebookSubscriber, null);
		Subscribe(subscriber as IInitiatorRulebookSubscriber, null);
		Hooks.Register(subscriber);
	}

	internal static void Unsubscribe([CanBeNull] object subscriber)
	{
		if (subscriber is ISubscriptionProxy subscriptionProxy)
		{
			Unsubscribe(subscriptionProxy.GetSubscriber() as IGlobalRulebookSubscriber, subscriptionProxy);
			Unsubscribe(subscriptionProxy.GetSubscriber() as ITargetRulebookSubscriber, subscriptionProxy);
			Unsubscribe(subscriptionProxy.GetSubscriber() as IInitiatorRulebookSubscriber, subscriptionProxy);
		}
		Unsubscribe(subscriber as IGlobalRulebookSubscriber, null);
		Unsubscribe(subscriber as ITargetRulebookSubscriber, null);
		Unsubscribe(subscriber as IInitiatorRulebookSubscriber, null);
		Hooks.Unregister(subscriber);
	}

	private static void Subscribe([CanBeNull] IGlobalRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalRulebookSubscribers.Subscribe(subscriber, proxy);
		}
	}

	private static void Unsubscribe([CanBeNull] IGlobalRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			GlobalRulebookSubscribers.Unsubscribe(subscriber, proxy);
		}
	}

	private static void Subscribe([CanBeNull] ITargetRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
			if (entity == null)
			{
				PFLog.Default.Error("Could not subscribe {0}, it didnt provide a unit", subscriber);
			}
			else
			{
				TargetRulebookSubscribers.Sure(entity).Subscribe(subscriber, proxy);
			}
		}
	}

	private static void Subscribe([CanBeNull] IInitiatorRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber != null)
		{
			IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
			if (entity == null)
			{
				PFLog.Default.Error("Could not subscribe {0}, it didnt provide a unit", subscriber);
			}
			else
			{
				InitiatorRulebookSubscribers.Sure(entity).Subscribe(subscriber, proxy);
			}
		}
	}

	private static void Unsubscribe([CanBeNull] ITargetRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
		if (entity == null)
		{
			PFLog.Default.Error("Could not unsubscribe {0}, it didnt provide a unit", subscriber);
			return;
		}
		RulebookSubscriptionManager<ITargetRulebookSubscriber> rulebookSubscriptionManager = TargetRulebookSubscribers.Get(entity);
		if (rulebookSubscriptionManager != null)
		{
			rulebookSubscriptionManager.Unsubscribe(subscriber, proxy);
			if (rulebookSubscriptionManager.Empty())
			{
				TargetRulebookSubscribers.Remove(entity);
			}
		}
	}

	private static void Unsubscribe([CanBeNull] IInitiatorRulebookSubscriber subscriber, [CanBeNull] ISubscriptionProxy proxy)
	{
		if (subscriber == null)
		{
			return;
		}
		IEntity entity = proxy?.GetSubscribingEntity() ?? subscriber.GetSubscribingEntity();
		if (entity == null)
		{
			PFLog.Default.Error("Could not unsubscribe {0}, it didnt provide a unit", subscriber);
			return;
		}
		RulebookSubscriptionManager<IInitiatorRulebookSubscriber> rulebookSubscriptionManager = InitiatorRulebookSubscribers.Get(entity);
		if (rulebookSubscriptionManager != null)
		{
			rulebookSubscriptionManager.Unsubscribe(subscriber, proxy);
			if (rulebookSubscriptionManager.Empty())
			{
				InitiatorRulebookSubscribers.Remove(entity);
			}
		}
	}

	internal static void ClearEntitySubscriptions([NotNull] IEntity unit)
	{
		TargetRulebookSubscribers.Remove(unit);
		InitiatorRulebookSubscribers.Remove(unit);
	}

	public static void OnEventAboutToTrigger<T>(T evt) where T : IRulebookEvent
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			using (ContextData<RulebookContextData>.Request().Setup(evt))
			{
				Hooks.OnBeforeEventAboutToTrigger(evt);
				GlobalRulebookSubscribers.OnEventAboutToTrigger(evt);
				IMechanicEntity ruleTarget = evt.GetRuleTarget();
				if (ruleTarget != null)
				{
					TargetRulebookSubscribers.Get(ruleTarget)?.OnEventAboutToTrigger(evt);
				}
				InitiatorRulebookSubscribers.Get(evt.Initiator)?.OnEventAboutToTrigger(evt);
			}
		}
	}

	public static void OnEventDidTrigger<T>(T evt) where T : IRulebookEvent
	{
		using (ContextData<StackOverflowProtection>.Request())
		{
			using (ContextData<RulebookContextData>.Request().Setup(evt))
			{
				GlobalRulebookSubscribers.OnEventDidTrigger(evt);
				IMechanicEntity ruleTarget = evt.GetRuleTarget();
				if (ruleTarget != null)
				{
					TargetRulebookSubscribers.Get(ruleTarget)?.OnEventDidTrigger(evt);
				}
				InitiatorRulebookSubscribers.Get(evt.Initiator)?.OnEventDidTrigger(evt);
				Hooks.OnAfterEventDidTrigger(evt);
			}
		}
	}
}
