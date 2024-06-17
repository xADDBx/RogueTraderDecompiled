using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

public class TimeService
{
	private float m_TimeAccumulator;

	public bool WillSimulateOnCurrentFrame { get; private set; }

	public void Tick(UpdateMode updateMode)
	{
		WillSimulateOnCurrentFrame = false;
		if ((updateMode != 0 || !(Time.timeScale <= 0f)) && (updateMode != UpdateMode.FixedUpdateFrequencyWithPause || !(Time.timeScale <= 0f)))
		{
			m_TimeAccumulator += Time.unscaledDeltaTime;
			if (m_TimeAccumulator >= PBD.UnscaledUpdatePeriod)
			{
				m_TimeAccumulator = 0f;
				WillSimulateOnCurrentFrame = true;
				PBD.OnBeforeSimulationTick?.Invoke();
			}
		}
	}
}
