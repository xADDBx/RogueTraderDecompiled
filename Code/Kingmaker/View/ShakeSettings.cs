using Kingmaker.Utility.Random;
using UnityEngine;

namespace Kingmaker.View;

public class ShakeSettings
{
	private float m_Amplitude;

	private float m_Speed;

	private Vector3 m_PrevShift;

	private Vector3 m_Direction;

	private float m_TotalTime;

	private float m_TimePassed;

	private bool m_ShouldBeActive;

	public Vector3 CurrentShift => m_PrevShift + m_Direction * (m_TimePassed * m_Speed);

	public bool Active
	{
		get
		{
			return m_ShouldBeActive;
		}
		set
		{
			if (m_ShouldBeActive != value)
			{
				m_ShouldBeActive = value;
				GetNextPoint();
			}
		}
	}

	public void Start(float amplitude, float speed)
	{
		m_Amplitude = amplitude;
		m_Speed = speed;
		if (!Active)
		{
			m_TimePassed = 0f;
			m_PrevShift = Vector3.zero;
			m_Direction = Vector3.zero;
			m_TotalTime = 0f;
		}
		m_ShouldBeActive = true;
		GetNextPoint();
	}

	private void GetNextPoint()
	{
		m_PrevShift = CurrentShift;
		Vector3 vector = (m_ShouldBeActive ? (PFStatefulRandom.Camera.insideUnitSphere * m_Amplitude) : Vector3.zero) - m_PrevShift;
		float magnitude = vector.magnitude;
		m_TotalTime = ((magnitude > 0f) ? (magnitude / m_Speed) : 0f);
		m_Direction = ((magnitude > 0f) ? (vector / magnitude) : Vector3.zero);
		m_TimePassed = 0f;
	}

	public void Tick(float dt)
	{
		if (Active)
		{
			m_TimePassed += dt;
			if (m_TimePassed >= m_TotalTime)
			{
				GetNextPoint();
			}
		}
	}
}
