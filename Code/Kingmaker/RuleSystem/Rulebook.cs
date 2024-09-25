using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.RuleSystem;

public class Rulebook : IRulebookTrigger
{
	private static Rulebook s_Instance;

	private static RulebookEventContext m_RuleContext;

	public static Rulebook Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new Rulebook();
				m_RuleContext = new RulebookEventContext(s_Instance);
			}
			return s_Instance;
		}
	}

	public RulebookEventContext Context => m_RuleContext;

	public static RulebookEventContext CurrentContext => Instance.Context;

	private void TriggerEventInternal<T>(T evt) where T : IRulebookEvent
	{
		AssignSource(evt);
		try
		{
			RulebookEventBus.OnEventAboutToTrigger(evt);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		try
		{
			evt.OnTrigger(m_RuleContext);
		}
		catch (Exception ex2)
		{
			PFLog.Default.Exception(ex2);
		}
		try
		{
			RulebookEventBus.OnEventDidTrigger(evt);
			evt.OnDidTrigger();
		}
		catch (Exception ex3)
		{
			PFLog.Default.Exception(ex3);
		}
	}

	private void AssignSource(IRulebookEvent rulebookEvent)
	{
		RulebookEvent rulebookEvent2 = (RulebookEvent)rulebookEvent;
		RuleReason left = rulebookEvent2.Reason;
		RuleReason right = default(RuleReason);
		if (left != right)
		{
			return;
		}
		if (rulebookEvent2 is RulePerformAbility)
		{
			rulebookEvent2.Reason = rulebookEvent2;
			return;
		}
		MechanicEntityFact fact = ContextDataHelper.GetFact();
		if (fact != null)
		{
			rulebookEvent2.Reason = fact;
			return;
		}
		RuleReason? ruleReason = ((RulebookEvent)m_RuleContext.Previous)?.Reason;
		MechanicsContext mechanicsContext = ContextData<MechanicsContext.Data>.Current?.Context;
		if (ruleReason.HasValue)
		{
			rulebookEvent2.Reason = ruleReason.Value.Copy(mechanicsContext);
		}
		else if (mechanicsContext != null)
		{
			rulebookEvent2.Reason = mechanicsContext;
		}
		else
		{
			rulebookEvent2.Reason = rulebookEvent2;
		}
	}

	public TEvent TriggerEvent<TEvent>(TEvent evt) where TEvent : IRulebookEvent
	{
		if (evt.IsTriggered)
		{
			return evt;
		}
		bool flag = m_RuleContext.Current == null;
		m_RuleContext.PushEvent(evt);
		using (ContextData<GameLogDisabled>.RequestIf(evt.IsGameLogDisabled))
		{
			TriggerEventInternal(evt);
		}
		m_RuleContext.PopEvent(evt);
		if (flag)
		{
			m_RuleContext = new RulebookEventContext(Instance);
		}
		return evt;
	}

	public void TriggerEventInRestoredContext<TEvent>(RulebookEventContext context, TEvent evt) where TEvent : IRulebookEvent
	{
		RulebookEventContext ruleContext = m_RuleContext;
		m_RuleContext = context;
		TriggerEvent(evt);
		m_RuleContext = ruleContext;
	}

	public static TEvent Trigger<TEvent>([NotNull] TEvent evt) where TEvent : IRulebookEvent
	{
		return Instance.TriggerEvent(evt);
	}

	public TEvent Trigger<TEvent>([NotNull] RulebookEventContext context, [NotNull] TEvent evt) where TEvent : IRulebookEvent
	{
		Instance.TriggerEventInRestoredContext(context, evt);
		return evt;
	}
}
