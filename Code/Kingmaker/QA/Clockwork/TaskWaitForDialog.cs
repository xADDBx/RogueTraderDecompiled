using System.Collections;
using Kingmaker.GameModes;

namespace Kingmaker.QA.Clockwork;

internal class TaskWaitForDialog : ClockworkRunnerTask
{
	private float m_MaxWaitTime = 5f;

	private float m_TimeLeft;

	public TaskWaitForDialog(ClockworkRunner runner)
		: base(runner)
	{
		m_TimeLeft = m_MaxWaitTime * runner.TimeScale;
	}

	protected override IEnumerator Routine()
	{
		while (m_TimeLeft > 0f)
		{
			if (Game.Instance.CurrentMode == GameModeType.Dialog)
			{
				yield break;
			}
			m_TimeLeft -= Game.Instance.TimeController.DeltaTime;
			yield return null;
		}
		if (Game.Instance.CurrentMode != GameModeType.Dialog)
		{
			float taskTimeout = Clockwork.Instance.Scenario.TaskTimeout;
			Clockwork.Instance.Reporter.HandleError("Cannot create answer task - game is not in dialog mode!");
			yield return new TaskDelayedCall(Runner, null, "AnswerCommand: Cannot create answer task - game is not in dialog mode!", taskTimeout);
		}
	}

	public override string ToString()
	{
		float num = m_TimeLeft % Runner.TimeScale;
		num = ((num > 0f) ? num : 0f);
		return $"Waiting for dialog ({num:F1} seconds left)";
	}
}
