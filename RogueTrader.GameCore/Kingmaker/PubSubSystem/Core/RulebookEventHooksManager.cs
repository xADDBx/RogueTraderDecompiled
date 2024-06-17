using System;
using System.Collections.Generic;

namespace Kingmaker.PubSubSystem.Core;

public class RulebookEventHooksManager
{
	private readonly List<IRulebookEventAboutToTriggerHook> m_AboutToTriggerHooks = new List<IRulebookEventAboutToTriggerHook>();

	private readonly List<IRulebookEventDidTriggerHook> m_DidTriggerHooks = new List<IRulebookEventDidTriggerHook>();

	public void Register(object hook)
	{
		Register(hook as IRulebookEventAboutToTriggerHook, m_AboutToTriggerHooks, "m_AboutToTriggerHooks");
		Register(hook as IRulebookEventDidTriggerHook, m_DidTriggerHooks, "m_DidTriggerHooks");
	}

	public void Unregister(object hook)
	{
		Unregister(hook as IRulebookEventAboutToTriggerHook, m_AboutToTriggerHooks, "m_AboutToTriggerHooks");
		Unregister(hook as IRulebookEventDidTriggerHook, m_DidTriggerHooks, "m_DidTriggerHooks");
	}

	private static void Register<T>(T hook, List<T> list, string listName) where T : IRulebookEventHook
	{
		if (hook != null)
		{
			if (list.Contains(hook))
			{
				PFLog.Default.Error($"{listName} already contains hook: {hook}");
			}
			else
			{
				list.Add(hook);
			}
		}
	}

	private static void Unregister<T>(T hook, List<T> list, string listName) where T : IRulebookEventHook
	{
		if (hook != null && !list.Remove(hook))
		{
			PFLog.Default.Error($"{listName} does not contains: {hook}");
		}
	}

	public void OnBeforeEventAboutToTrigger<T>(T rule) where T : IRulebookEvent
	{
		foreach (IRulebookEventAboutToTriggerHook aboutToTriggerHook in m_AboutToTriggerHooks)
		{
			try
			{
				aboutToTriggerHook.OnBeforeEventAboutToTrigger(rule);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}

	public void OnAfterEventDidTrigger<T>(T rule) where T : IRulebookEvent
	{
		foreach (IRulebookEventDidTriggerHook didTriggerHook in m_DidTriggerHooks)
		{
			try
			{
				didTriggerHook.OnAfterEventDidTrigger(rule);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
			}
		}
	}
}
