using System;
using System.Collections;

namespace Kingmaker.QA.Clockwork;

internal class TaskDelayedCall : ClockworkRunnerTask
{
	private string m_Message = "Delayed call or wait";

	private float m_WaitTime = 15f;

	private Action m_Action;

	public TaskDelayedCall(ClockworkRunner runner, Action action = null, string message = null, float delayTime = 15f)
		: base(runner)
	{
		m_Action = action;
		if (message != null)
		{
			m_Message = message;
		}
		m_WaitTime = delayTime;
	}

	protected override IEnumerator Routine()
	{
		yield return m_WaitTime;
		m_Action?.Invoke();
		yield return null;
	}

	public override string ToString()
	{
		return m_Message;
	}
}
