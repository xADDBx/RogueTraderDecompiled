using System;
using System.Collections;
using Kingmaker.Blueprints;
using UnityEngine;

namespace Kingmaker.QA.Clockwork;

public abstract class ClockworkRunnerTask
{
	private ClockworkCommand m_SourceCommand;

	private float m_CompleteDelay;

	private Func<bool> m_CheckCompleteCondition = () => true;

	private bool m_AttemptSaved;

	public readonly ClockworkRunner Runner;

	public float TimeLeft;

	public IEnumerator Ticker { get; private set; }

	public bool IsTimeout => TimeLeft <= 0f;

	public ClockworkRunnerTask Parent { get; set; }

	protected ClockworkRunnerTask(ClockworkRunner runner)
	{
		Runner = runner;
		Clockwork instance = Clockwork.Instance;
		TimeLeft = ((instance == null) ? null : SimpleBlueprintExtendAsObject.Or(instance.Scenario, null)?.TaskTimeout) ?? 600f;
	}

	public void SetCompleteCondition(Func<bool> CheckCompleteCondition)
	{
		m_CheckCompleteCondition = CheckCompleteCondition;
	}

	public void SetSourceCommand(ClockworkCommand sourceCommand)
	{
		m_SourceCommand = sourceCommand;
	}

	public void Complete()
	{
		if (m_CheckCompleteCondition())
		{
			m_SourceCommand?.Complete();
		}
	}

	public void UpdateTimer()
	{
		TimeLeft -= Time.unscaledDeltaTime;
	}

	public virtual bool TooManyAttempts()
	{
		Clockwork instance = Clockwork.Instance;
		object obj;
		if (instance == null)
		{
			obj = null;
		}
		else
		{
			ClockworkRunner runner = instance.Runner;
			obj = ((runner == null) ? null : ElementExtendAsObject.Or(runner.LastCommand, null)?.name);
		}
		if (obj == null)
		{
			obj = "NoCommand";
		}
		string target = (string)obj + ToString();
		if (!m_AttemptSaved)
		{
			m_AttemptSaved = true;
			AttemptsCounter.Instance.AddAttempt(target);
		}
		bool num = AttemptsCounter.Instance.CheckTooManyAttempts(target);
		if (num)
		{
			PFLog.Clockwork.Error($"Reached max attempts count for Clockwork task: {this}");
		}
		return num;
	}

	public void CreateTicker()
	{
		Ticker = Routine();
	}

	protected abstract IEnumerator Routine();

	public virtual void Cleanup()
	{
	}
}
