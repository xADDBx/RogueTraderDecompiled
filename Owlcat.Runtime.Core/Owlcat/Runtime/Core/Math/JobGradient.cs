using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

public struct JobGradient
{
	[ReadOnly]
	private NativeArray<GradientColorKey> m_ColorKeys;

	[ReadOnly]
	private NativeArray<GradientAlphaKey> m_AlphaKeys;

	public JobGradient(Gradient gradient)
	{
		GradientColorKey[] colorKeys = gradient.colorKeys;
		Array.Sort(colorKeys, SortColorKeys);
		GradientAlphaKey[] alphaKeys = gradient.alphaKeys;
		Array.Sort(alphaKeys, SortAlphaKeys);
		m_ColorKeys = new NativeArray<GradientColorKey>(colorKeys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_AlphaKeys = new NativeArray<GradientAlphaKey>(alphaKeys.Length, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_ColorKeys.CopyFrom(colorKeys);
		m_AlphaKeys.CopyFrom(alphaKeys);
		static int SortAlphaKeys(GradientAlphaKey a, GradientAlphaKey b)
		{
			return a.time.CompareTo(b.time);
		}
		static int SortColorKeys(GradientColorKey a, GradientColorKey b)
		{
			return a.time.CompareTo(b.time);
		}
	}

	public Color Evaluate(float time)
	{
		FindSurroundingKeyframes(time, out var lhsColorKey, out var rhsColorKey, out var lhsAlphaKey, out var rhsAlphaKey);
		float time2 = lhsColorKey.time;
		float num = rhsColorKey.time - time2;
		Color result;
		if (num != 0f)
		{
			float t = (time - time2) / num;
			result = Color.Lerp(lhsColorKey.color, rhsColorKey.color, t);
		}
		else
		{
			result = lhsColorKey.color;
		}
		time2 = lhsAlphaKey.time;
		num = rhsAlphaKey.time - time2;
		if (num != 0f)
		{
			float t2 = (time - time2) / num;
			result.a = math.lerp(lhsAlphaKey.alpha, rhsAlphaKey.alpha, t2);
		}
		else
		{
			result.a = lhsAlphaKey.alpha;
		}
		return result;
	}

	private void FindSurroundingKeyframes(float time, out GradientColorKey lhsColorKey, out GradientColorKey rhsColorKey, out GradientAlphaKey lhsAlphaKey, out GradientAlphaKey rhsAlphaKey)
	{
		int num = m_ColorKeys.Length;
		int num2 = 0;
		while (num > 0)
		{
			int num3 = num >> 1;
			int num4 = num2 + num3;
			if (time < m_ColorKeys[num4].time)
			{
				num = num3;
				continue;
			}
			num2 = num4 + 1;
			num = num - num3 - 1;
		}
		lhsColorKey = m_ColorKeys[num2 - 1];
		rhsColorKey = m_ColorKeys[math.min(m_ColorKeys.Length - 1, num2)];
		num = m_AlphaKeys.Length;
		num2 = 0;
		while (num > 0)
		{
			int num3 = num >> 1;
			int num4 = num2 + num3;
			if (time < m_AlphaKeys[num4].time)
			{
				num = num3;
				continue;
			}
			num2 = num4 + 1;
			num = num - num3 - 1;
		}
		lhsAlphaKey = m_AlphaKeys[num2 - 1];
		rhsAlphaKey = m_AlphaKeys[math.min(m_AlphaKeys.Length - 1, num2)];
	}

	public void Dispose()
	{
		if (m_AlphaKeys.IsCreated)
		{
			m_AlphaKeys.Dispose();
		}
		if (m_ColorKeys.IsCreated)
		{
			m_ColorKeys.Dispose();
		}
	}
}
