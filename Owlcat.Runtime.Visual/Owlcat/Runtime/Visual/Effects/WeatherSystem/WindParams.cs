using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public struct WindParams
{
	[SerializeField]
	private Vector2 m_Direction;

	[SerializeField]
	private float m_Intensity;

	public static WindParams Zero { get; } = new WindParams(Vector2.right);


	public Vector2 Direction => m_Direction;

	public float Intensity => m_Intensity;

	public WindParams(Vector2 direction, float intensity)
	{
		m_Direction = direction;
		m_Intensity = intensity;
		ValidateAndNormalize();
	}

	public WindParams(Vector2 vector)
	{
		m_Direction = vector;
		m_Intensity = vector.magnitude;
		ValidateAndNormalize();
	}

	private void ValidateAndNormalize()
	{
		if (Mathf.Abs(m_Direction.magnitude) < 0.0001f)
		{
			m_Direction = Vector2.right;
		}
		m_Direction = m_Direction.normalized;
		if (m_Intensity < 0.1f)
		{
			m_Intensity = 0.1f;
		}
	}

	public Vector2 GetVector()
	{
		return m_Direction * m_Intensity;
	}

	public float2 GetFloat2()
	{
		return new float2(GetVector());
	}

	public Vector2 GetVectorNormalized()
	{
		return m_Direction;
	}

	public float2 GetFloat2Normalized()
	{
		return new float2(GetVectorNormalized());
	}

	public static WindParams Lerp(WindParams a, WindParams b, float t)
	{
		return new WindParams(Vector2.Lerp(a.m_Direction, b.m_Direction, t).normalized, Mathf.Lerp(a.m_Intensity, b.m_Intensity, t));
	}

	public static WindParams Sample(WindProfile profile, InclemencyType inclemency)
	{
		return new WindParams(UnityEngine.Random.insideUnitCircle, profile?.WindIntensityRanges.Sample(inclemency) ?? 0f);
	}
}
