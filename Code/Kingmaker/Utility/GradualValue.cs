using UnityEngine;

namespace Kingmaker.Utility;

public class GradualValue
{
	private float m_Target;

	private float m_Current;

	private float m_LastUpdateTime;

	private float m_CurrentSpeed;

	public float Speed { get; set; }

	public float MaxTime { get; set; }

	public float Target
	{
		get
		{
			return m_Target;
		}
		set
		{
			if (value != m_Target)
			{
				Update();
				m_Target = value;
				if (MaxTime > 0f)
				{
					m_CurrentSpeed = Mathf.Max(Speed, Mathf.Abs((m_Target - m_Current) / MaxTime));
				}
			}
		}
	}

	public float Current
	{
		get
		{
			Update();
			return m_Current;
		}
	}

	public GradualValue(float start)
	{
		Speed = 1f;
		m_Target = (m_Current = start);
		m_LastUpdateTime = GetTime();
	}

	public void Update()
	{
		float time = GetTime();
		if (m_Current == Target || time == m_LastUpdateTime)
		{
			m_LastUpdateTime = time;
			return;
		}
		float num = Mathf.Sign(Target - m_Current) * (time - m_LastUpdateTime) * ((MaxTime > 0f) ? m_CurrentSpeed : Speed);
		m_Current += num;
		if (num > 0f && m_Current > Target)
		{
			m_Current = Target;
		}
		if (num < 0f && m_Current < Target)
		{
			m_Current = Target;
		}
		m_LastUpdateTime = time;
	}

	public static implicit operator float(GradualValue value)
	{
		return value.Current;
	}

	private float GetTime()
	{
		return (float)Game.Instance.TimeController.RealTime.TotalSeconds;
	}
}
