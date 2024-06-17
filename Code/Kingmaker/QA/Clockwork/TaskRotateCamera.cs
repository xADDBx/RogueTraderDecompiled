using System.Collections;
using Kingmaker.View;

namespace Kingmaker.QA.Clockwork;

public class TaskRotateCamera : ClockworkRunnerTask
{
	private float m_Rotation;

	private float m_WaitTime;

	public TaskRotateCamera(ClockworkRunner runner)
		: base(runner)
	{
		CameraRig instance = CameraRig.Instance;
		m_Rotation = instance.transform.rotation.eulerAngles.y + 90f;
		m_WaitTime = 5f;
	}

	public TaskRotateCamera(ClockworkRunner runner, float rotation, float waitTime = 10f)
		: base(runner)
	{
		m_Rotation = rotation;
		m_WaitTime = waitTime;
	}

	protected override IEnumerator Routine()
	{
		yield return 1f;
		CameraRig.Instance.RotateTo(m_Rotation);
		yield return m_WaitTime * (Runner?.TimeScale ?? 1f);
	}

	public override bool TooManyAttempts()
	{
		return false;
	}

	public override string ToString()
	{
		return $"Rotate Camera to position {m_Rotation} and wait {m_WaitTime} seconds";
	}
}
