using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

public struct JobAnimationCurve
{
	[ReadOnly]
	private NativeArray<Keyframe> m_Keyframes;

	public JobAnimationCurve(AnimationCurve curve)
	{
		Keyframe[] keys = curve.keys;
		Array.Sort(keys, SortByTime);
		m_Keyframes = new NativeArray<Keyframe>(keys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_Keyframes.CopyFrom(keys);
		static int SortByTime(Keyframe a, Keyframe b)
		{
			return a.time.CompareTo(b.time);
		}
	}

	public float EvaluateNormalized(float time)
	{
		float time2 = m_Keyframes[m_Keyframes.Length - 1].time;
		float num = Mathf.Clamp01(time);
		return Evaluate(time2 * num);
	}

	public float Evaluate(float time)
	{
		FindSurroundingKeyframes(time, out var lhsKeyframe, out var rhsKeyframe);
		return HermiteInterpolate(time, lhsKeyframe.time, in lhsKeyframe, rhsKeyframe.time, in rhsKeyframe);
	}

	private void FindSurroundingKeyframes(float time, out Keyframe lhsKeyframe, out Keyframe rhsKeyframe)
	{
		int num = m_Keyframes.Length;
		int num2 = 0;
		while (num > 0)
		{
			int num3 = num >> 1;
			int num4 = num2 + num3;
			if (time < m_Keyframes[num4].time)
			{
				num = num3;
				continue;
			}
			num2 = num4 + 1;
			num = num - num3 - 1;
		}
		lhsKeyframe = m_Keyframes[num2 - 1];
		rhsKeyframe = m_Keyframes[math.min(m_Keyframes.Length - 1, num2)];
	}

	private static float HermiteInterpolate(float time, float leftTime, in Keyframe leftKeyframe, float rightTime, in Keyframe rightKeyframe)
	{
		if (math.isinf(leftKeyframe.outTangent) || math.isinf(rightKeyframe.inTangent))
		{
			return leftKeyframe.value;
		}
		float num = rightTime - leftTime;
		float t;
		float m;
		float m2;
		if (num != 0f)
		{
			t = (time - leftTime) / num;
			m = leftKeyframe.outTangent * num;
			m2 = rightKeyframe.inTangent * num;
		}
		else
		{
			t = 0f;
			m = 0f;
			m2 = 0f;
		}
		return HermiteInterpolate(t, leftKeyframe.value, m, m2, rightKeyframe.value);
	}

	private static float HermiteInterpolate(float t, float p0, float m0, float m1, float p1)
	{
		float num = 2f * p0 + m0 - 2f * p1 + m1;
		float num2 = -3f * p0 - 2f * m0 + 3f * p1 - m1;
		return t * (t * (num * t + num2) + m0) + p0;
	}

	public void Dispose()
	{
		if (m_Keyframes.IsCreated)
		{
			m_Keyframes.Dispose();
		}
	}
}
