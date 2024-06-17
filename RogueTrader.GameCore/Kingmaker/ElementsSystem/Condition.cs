using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem.Interfaces;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
public abstract class Condition : Element
{
	[HideInInspector]
	public bool Not;

	public bool? LastResult { get; private set; }

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
		try
		{
			return (Not ? "Not " : "") + GetConditionCaption();
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			return (Not ? "Not " : "") + GetType().Name;
		}
	}

	protected abstract string GetConditionCaption();

	public bool Check([CanBeNull] IConditionDebugContext debugContext = null)
	{
		bool flag = (Not ? (!CheckCondition()) : CheckCondition());
		if (debugContext != null)
		{
			string text = ToString();
			Color color = (flag ? Color.green : Color.red);
			debugContext.AddConditionDebugMessage((flag ? "passed: " : "failed: ") + text, color);
		}
		LastResult = flag;
		return flag;
	}

	protected abstract bool CheckCondition();
}
