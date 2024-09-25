using System;
using Kingmaker.Utility.StatefulRandom;
using UnityEngine;

namespace Kingmaker.Visual.Animation.Kingmaker.Actions;

[Serializable]
public class TimedProbabilityCurve
{
	public class Tracker
	{
		private float m_Random;

		private float m_Time;

		private TimedProbabilityCurve m_Curve;

		public Tracker(TimedProbabilityCurve curve, StatefulRandom statefulRandom)
		{
			m_Curve = curve;
			m_Random = statefulRandom.value;
		}

		public bool Tick(float dt)
		{
			m_Random -= dt * m_Curve.Get(m_Time);
			m_Time += dt;
			return m_Random <= 0f;
		}
	}

	[SerializeField]
	private float m_Integral = 23f;

	[SerializeField]
	private float m_MinTime = 7f;

	[SerializeField]
	private float m_MaxTime;

	private TimedProbabilityCurve()
	{
		m_MaxTime = m_MinTime + m_Integral;
	}

	public float Get(float t)
	{
		if (!(m_Integral > 0f) || !(t >= m_MinTime))
		{
			return 0f;
		}
		return 1f / m_Integral;
	}

	public Tracker Track(StatefulRandom statefulRandom)
	{
		return new Tracker(this, statefulRandom);
	}
}
