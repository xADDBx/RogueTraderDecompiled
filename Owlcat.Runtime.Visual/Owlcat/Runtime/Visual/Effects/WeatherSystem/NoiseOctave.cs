using System;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class NoiseOctave
{
	public float Weight;

	public float Scale;

	public float MoveSpeed;

	private float2 m_AccumulateShift;

	public static NoiseOctave Zero { get; } = new NoiseOctave(0f, 1f, 1f);


	public NoiseOctave(float weight, float scale, float moveSpeed)
	{
		Weight = weight;
		Scale = scale;
		MoveSpeed = moveSpeed;
		m_AccumulateShift = float2.zero;
	}

	public void Tick(float2 direction, float delta)
	{
		m_AccumulateShift += direction * MoveSpeed * Scale * delta;
	}

	public float4 Compressed()
	{
		return new float4(Weight, Scale, m_AccumulateShift);
	}

	public static NoiseOctave Lerp(NoiseOctave a, NoiseOctave b, float t)
	{
		return new NoiseOctave(Mathf.Lerp(a.MoveSpeed, b.MoveSpeed, t), Mathf.Lerp(a.Scale, b.Scale, t), Mathf.Lerp(a.Weight, b.Weight, t));
	}
}
