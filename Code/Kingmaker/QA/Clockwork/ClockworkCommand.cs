using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

[Serializable]
[TypeId("4eaeb7c894a3c93499555f32b027f1ab")]
public abstract class ClockworkCommand : Element
{
	protected bool m_IsCompleted;

	public bool IsCompleted => m_IsCompleted;

	public virtual void Initialize()
	{
		m_IsCompleted = false;
	}

	public void Complete()
	{
		m_IsCompleted = true;
		PFLog.Clockwork.Log($"Command complete: {this}");
	}

	public virtual ClockworkRunnerTask GetTask(ClockworkRunner runner)
	{
		return new TaskDummy(runner);
	}

	public string GetStatusString()
	{
		if (!Application.isPlaying)
		{
			return "";
		}
		string text = "";
		return IsCompleted ? "<color=green>[done]</color> " : text;
	}
}
