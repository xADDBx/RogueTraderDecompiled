using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.WeatherSystem;

[Serializable]
public class NoiseSettings
{
	public const int kOctavesCount = 2;

	[SerializeField]
	private NoiseOctave m_Octave1;

	[SerializeField]
	private NoiseOctave m_Octave2;

	protected IEnumerable<NoiseOctave> Octaves()
	{
		yield return m_Octave1;
		yield return m_Octave2;
	}

	public void Tick(float2 direction, float delta)
	{
		foreach (NoiseOctave item in Octaves())
		{
			item.Tick(direction, delta);
		}
	}

	public float4[] GetWeightedCompressed(float maxSumWeight = 1f)
	{
		float4[] array = new float4[2];
		float num = 0f;
		int num2 = 0;
		foreach (NoiseOctave item in Octaves())
		{
			num += item.Weight;
			array[num2] = item.Compressed();
			num2++;
		}
		for (int i = 0; i < array.Length; i++)
		{
			array[i].x /= num;
		}
		return array;
	}

	public float4[] GetCompressed()
	{
		float4[] array = new float4[2];
		int num = 0;
		foreach (NoiseOctave item in Octaves())
		{
			array[num] = item.Compressed();
			num++;
		}
		return array;
	}
}
