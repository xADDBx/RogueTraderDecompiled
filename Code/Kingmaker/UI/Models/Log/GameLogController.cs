using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UI.Models.Log.CombatLog_ThreadSystem;
using Kingmaker.UI.Models.Log.Events;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.AddPatterns;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.MergeEvent.Comparers;
using Kingmaker.UI.Models.Log.GameLogEventInsertPatterns.PostAddPatterns;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;

namespace Kingmaker.UI.Models.Log;

public class GameLogController : IControllerStart, IController, IControllerEnable, IControllerDisable, IControllerTick, IRulebookEventAboutToTriggerHook, IRulebookEventHook, IRulebookEventDidTriggerHook
{
	public abstract class GameEventsHandler
	{
		private GameLogController m_Controller;

		public void Setup(GameLogController controller)
		{
			m_Controller = controller;
		}

		protected void AddEvent(GameLogEvent @event)
		{
			m_Controller.AddEventToQueue(@event);
		}

		[CanBeNull]
		protected T GetEventFromQueue<T>(Func<T, bool> pred) where T : GameLogEvent
		{
			return m_Controller.GetEventFromQueue(pred);
		}
	}

	private static readonly Type[] GameEventsHandlerTypes;

	private readonly HashSet<Type> m_RootRulesTypes = new HashSet<Type>();

	private readonly Dictionary<Type, List<LogThreadBase>> m_EventToThreads = new Dictionary<Type, List<LogThreadBase>>();

	private readonly List<GameEventsHandler> m_GameEventsHandlers = new List<GameEventsHandler>();

	private readonly Stack<GameLogEvent> m_RuleEventsStack = new Stack<GameLogEvent>();

	private readonly List<GameLogEvent> m_EventsQueue = new List<GameLogEvent>();

	static GameLogController()
	{
		GameEventsHandlerTypes = (from i in typeof(GameEventsHandler).GetSubclasses()
			where i.GetConstructor(Type.EmptyTypes) != null
			select i).ToArray();
	}

	void IControllerStart.OnStart()
	{
		PatternCollection.Instance.Cleanup();
		PatternCollection.Instance.AddPattern(PatternAddEventMergeEvent<GameLogRuleEvent<RulePerformSavingThrow>>.Create(PerformSavingThrowComparer.Create())).AddPattern(PatternAddEventMergeEvent<GameLogEventItemsCollection>.Create(ItemsCollectionComparer.Create())).AddPattern(PatternAddEventMergeEvent<GameLogEventCargoCollection>.Create(CargoCollectionComparer.Create()))
			.AddPattern(PatternAddEventMergeEvent<GameLogRuleEvent<RuleStarshipPerformAttack>>.Create(PerformStarshipAttackComparer.Create()))
			.AddPattern(PatternAddEventMergeEvent<GameLogRuleEvent<RuleCalculateCanApplyBuff>>.Create(CalculateCanApplyBuffComparer.Create()))
			.AddPattern(PatternAddEventInsertSeparator.Create())
			.AddPattern(PatternAddEventConvertToEventScatterDealDamage.Create())
			.AddPattern(PatternAddEventConvertToEventAoeDealDamage.Create());
		PatternCollection.Instance.AddPattern(PatternPostAddEventAttackEventChildrenSort.Create()).AddPattern(PatternPostAddEventGrenadeAttackEventChildrenSort.Create()).AddPattern(PatternPostAddEventDeathMomentum.Create())
			.AddPattern(PatternPostAddEventSwitchApplyBuffAndDependAbility.Create())
			.AddPattern(PatternPostAddEventRemoveDealDamage.Create());
		ClearEvents();
		m_GameEventsHandlers.Clear();
		Type[] gameEventsHandlerTypes = GameEventsHandlerTypes;
		for (int i = 0; i < gameEventsHandlerTypes.Length; i++)
		{
			GameEventsHandler gameEventsHandler = (GameEventsHandler)Activator.CreateInstance(gameEventsHandlerTypes[i]);
			gameEventsHandler.Setup(this);
			m_GameEventsHandlers.Add(gameEventsHandler);
		}
		Setup();
	}

	void IControllerEnable.OnEnable()
	{
		ClearEvents();
		foreach (GameEventsHandler gameEventsHandler in m_GameEventsHandlers)
		{
			EventBus.Subscribe(gameEventsHandler);
		}
	}

	void IControllerDisable.OnDisable()
	{
		ClearEvents();
		foreach (GameEventsHandler gameEventsHandler in m_GameEventsHandlers)
		{
			EventBus.Unsubscribe(gameEventsHandler);
		}
	}

	TickType IControllerTick.GetTickType()
	{
		return TickType.Any;
	}

	void IControllerTick.Tick()
	{
		using (GameLogContext.Scope)
		{
			for (int j = 0; j < m_EventsQueue.Count; j++)
			{
				GameLogEvent gameLogEvent = m_EventsQueue[j];
				if (!gameLogEvent.IsReady)
				{
					break;
				}
				m_EventsQueue[j] = null;
				if (!gameLogEvent.IsEnabled)
				{
					continue;
				}
				foreach (LogThreadBase item in m_EventToThreads.Get(gameLogEvent.GetType()))
				{
					try
					{
						gameLogEvent.Invoke(item);
					}
					catch (Exception ex)
					{
						PFLog.UI.Exception(ex);
					}
				}
			}
		}
		m_EventsQueue.RemoveAll((GameLogEvent i) => i == null);
	}

	void IRulebookEventAboutToTriggerHook.OnBeforeEventAboutToTrigger(IRulebookEvent rule)
	{
		bool flag = m_RootRulesTypes.Contains(rule.GetType());
		if (!m_RuleEventsStack.TryPeek(out var result) && !flag)
		{
			return;
		}
		GameLogEvent gameLogEvent = GameLogEventsFactory.Create((RulebookEvent)rule);
		bool flag2 = result?.TryAddInnerEvent(gameLogEvent) ?? false;
		if (!flag2)
		{
			for (int num = m_EventsQueue.Count - 1; num >= 0; num--)
			{
				GameLogEvent gameLogEvent2 = m_EventsQueue[num];
				if (!gameLogEvent2.IsReady && gameLogEvent2.TryAddInnerEvent(gameLogEvent))
				{
					flag2 = true;
					break;
				}
			}
		}
		if (flag2 || flag)
		{
			m_RuleEventsStack.Push(gameLogEvent);
		}
	}

	void IRulebookEventDidTriggerHook.OnAfterEventDidTrigger(IRulebookEvent rule)
	{
		if (m_RuleEventsStack.TryPeek(out var result) && result is GameLogRuleEvent gameLogRuleEvent && gameLogRuleEvent.Rule == rule)
		{
			m_RuleEventsStack.Pop();
			if (m_RootRulesTypes.Contains(rule.GetType()))
			{
				AddEventToQueue(gameLogRuleEvent);
			}
		}
	}

	private void ClearEvents()
	{
		m_RuleEventsStack.Clear();
		m_EventsQueue.Clear();
	}

	[CanBeNull]
	private T GetEventFromQueue<T>(Func<T, bool> pred) where T : GameLogEvent
	{
		return (T)m_EventsQueue.FirstItem((GameLogEvent i) => i is T arg && pred(arg));
	}

	private void AddEventToQueue(GameLogEvent @event)
	{
		GameLogEvent gameLogEvent = m_EventsQueue.LastItem();
		if (gameLogEvent != null)
		{
			if (@event.ParentEvent == null && gameLogEvent.TrySwallowEvent(@event))
			{
				return;
			}
			if (gameLogEvent.ParentEvent == null && @event.TrySwallowEvent(gameLogEvent))
			{
				m_EventsQueue.RemoveAt(m_EventsQueue.Count - 1);
			}
		}
		PatternCollection.Instance.ApplyPatterns(m_EventsQueue, @event);
	}

	private void Setup()
	{
		m_RootRulesTypes.Clear();
		m_EventToThreads.Clear();
		foreach (LogThreadBase allThread in Services.GetInstance<LogThreadService>().AllThreads)
		{
			Type[] interfaces = allThread.GetType().GetInterfaces();
			foreach (Type type in interfaces)
			{
				if (!type.IsGenericType)
				{
					continue;
				}
				Type type2 = null;
				Type type3 = null;
				if (type.GetGenericTypeDefinition() == typeof(IGameLogRuleHandler<>))
				{
					type2 = type.GetGenericArguments()[0];
					type3 = typeof(GameLogRuleEvent<>).MakeGenericType(type2);
				}
				else if (type.GetGenericTypeDefinition() == typeof(IGameLogEventHandler<>))
				{
					type3 = type.GetGenericArguments()[0];
					Type type4 = type3;
					while (type4 != null && type4 != typeof(GameLogEvent) && (!type4.IsGenericType || (type4.GetGenericTypeDefinition() != typeof(GameLogRuleEvent<>) && type4.GetGenericTypeDefinition() != typeof(GameLogRuleEvent<, >))))
					{
						type4 = type4.BaseType;
					}
					if ((object)type4 != null && type4.IsGenericType && (type4.GetGenericTypeDefinition() == typeof(GameLogRuleEvent<>) || type4.GetGenericTypeDefinition() == typeof(GameLogRuleEvent<, >)))
					{
						type2 = type4.GetGenericArguments()[0];
					}
				}
				if (type2 != null)
				{
					m_RootRulesTypes.Add(type2);
				}
				if (type3 != null)
				{
					List<LogThreadBase> list = m_EventToThreads.Get(type3);
					if (list == null)
					{
						list = new List<LogThreadBase>();
						m_EventToThreads.Add(type3, list);
					}
					list.Add(allThread);
				}
			}
		}
	}
}
