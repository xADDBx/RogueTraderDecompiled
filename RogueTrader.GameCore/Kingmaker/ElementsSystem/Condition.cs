using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.Interfaces;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
[TypeId("edc7fae212634b99b907e11d9ef82a2a")]
public abstract class Condition : Element
{
	[HideInInspector]
	public bool Not;

	protected abstract bool CheckCondition();

	protected abstract string GetConditionCaption();

	public bool Check()
	{
		return Check(null, null);
	}

	public bool Check([NotNull] IConditionDebugContext debugContext)
	{
		return Check(null, debugContext);
	}

	public bool Check([CanBeNull] ConditionsChecker list, [CanBeNull] IConditionDebugContext debugContext)
	{
		using ElementsDebugger elementsDebugger = ElementsDebugger.Scope(list, this);
		try
		{
			bool flag = Not ^ CheckCondition();
			elementsDebugger?.SetResult(flag ? 1 : 0);
			try
			{
				debugContext?.AddConditionDebugMessage(this, flag, "{0}: {1}", flag ? "passed" : "failed", this);
			}
			catch (Exception exception)
			{
				Element.LogException(exception);
			}
			return flag;
		}
		catch (Exception exception2)
		{
			Element.LogException(exception2);
			elementsDebugger?.SetException(exception2);
			throw;
		}
	}

	public override Color GetCaptionColor()
	{
		if (Not)
		{
			return Color.grey + Color.red;
		}
		return base.GetCaptionColor();
	}

	public override string GetCaption()
	{
		return GetCaption(useLineBreaks: false);
	}

	public override string GetCaption(bool useLineBreaks)
	{
		try
		{
			return (Not ? "Not " : "") + GetConditionCaption();
		}
		catch (Exception exception)
		{
			Element.LogException(exception);
			return (Not ? "Not " : "") + GetType().Name;
		}
	}
}
