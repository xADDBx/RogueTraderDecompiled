using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Scene;

public class Wind : MonoBehaviour
{
	[Serializable]
	public class NoiseOctave
	{
		public float Weight = 1f;

		public float Scale = 0.01f;

		public float MoveSpeed = 1f;

		[NonSerialized]
		public float2 AccumulatedVector;
	}

	private WindForce m_WindForce;

	public float2 Direction = new float2(1f, 0f);

	public float Intensity = 1f;

	[Range(0f, 1f)]
	public float StrengthNoiseWeight = 0.5f;

	[Range(1f, 10f)]
	public float StrengthNoiseContrast = 1f;

	public NoiseOctave StrenghtOctave0 = new NoiseOctave();

	public NoiseOctave StrengthOctave1 = new NoiseOctave();

	public NoiseOctave ShiftOctave0 = new NoiseOctave();

	public NoiseOctave ShiftOctave1 = new NoiseOctave();

	private NativeArray<float4> m_StrengthOctaves;

	private NativeArray<float4> m_ShiftOctaves;

	public NativeArray<float4> StrengthOctaves => m_StrengthOctaves;

	public NativeArray<float4> ShiftOctaves => m_ShiftOctaves;

	public IEnumerable<NoiseOctave> EnumerateStrenghtOctaves()
	{
		yield return StrenghtOctave0;
		yield return StrengthOctave1;
	}

	public IEnumerable<NoiseOctave> EnumerateShiftOctaves()
	{
		yield return ShiftOctave0;
		yield return ShiftOctave1;
	}

	private void OnEnable()
	{
		if (m_WindForce == null)
		{
			m_WindForce = new WindForce();
			m_StrengthOctaves = new NativeArray<float4>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			m_ShiftOctaves = new NativeArray<float4>(2, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		}
		PBD.RegisterForce(m_WindForce);
	}

	private void OnDisable()
	{
		PBD.UnregisterForce(m_WindForce);
		if (m_StrengthOctaves.IsCreated)
		{
			m_StrengthOctaves.Dispose();
		}
		if (m_ShiftOctaves.IsCreated)
		{
			m_ShiftOctaves.Dispose();
		}
		m_WindForce.DisableGlobalWind();
	}

	private void Update()
	{
		m_WindForce.WindVector = math.normalize(Direction) * Intensity;
		m_WindForce.StrengthNoiseWeight = StrengthNoiseWeight;
		m_WindForce.StrengthNoiseContrast = StrengthNoiseContrast;
		float num = 0f;
		foreach (NoiseOctave item in EnumerateStrenghtOctaves())
		{
			item.AccumulatedVector += m_WindForce.WindVector * item.MoveSpeed * item.Scale * Time.deltaTime;
			num += item.Weight;
		}
		int num2 = 0;
		foreach (NoiseOctave item2 in EnumerateStrenghtOctaves())
		{
			m_StrengthOctaves[num2] = new float4(item2.Weight / num, item2.Scale, item2.AccumulatedVector);
			num2++;
		}
		num = 0f;
		foreach (NoiseOctave item3 in EnumerateShiftOctaves())
		{
			item3.AccumulatedVector += m_WindForce.WindVector * item3.MoveSpeed * item3.Scale * Time.deltaTime;
			num += item3.Weight;
		}
		num2 = 0;
		foreach (NoiseOctave item4 in EnumerateShiftOctaves())
		{
			m_ShiftOctaves[num2] = new float4(item4.Weight / num, item4.Scale, item4.AccumulatedVector);
			num2++;
		}
		m_WindForce.CompressedStrengthOctaves = m_StrengthOctaves;
		m_WindForce.CompressedShiftOctaves = m_ShiftOctaves;
		m_WindForce.SetupGlobalShaderParameters();
	}
}
