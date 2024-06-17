using System;
using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

public class IntegrateFunc
{
	private Func<float, float> m_Func;

	private float[] m_Values;

	private float m_From;

	private float m_To;

	public float Total => m_Values[m_Values.Length - 1];

	public IntegrateFunc(Func<float, float> func, float from, float to, int steps)
	{
		m_Values = new float[steps + 1];
		m_Func = func;
		m_From = from;
		m_To = to;
		ComputeValues();
	}

	public float Evaluate(float x)
	{
		float num = Mathf.InverseLerp(m_From, m_To, x);
		int num2 = (int)(num * (float)m_Values.Length);
		int num3 = (int)(num * (float)m_Values.Length + 0.5f);
		if (num2 == num3 || num3 >= m_Values.Length)
		{
			return m_Values[num2];
		}
		float num4 = Mathf.InverseLerp(num2, num3, num * (float)m_Values.Length);
		return (1f - num4) * m_Values[num2] + num4 * m_Values[num3];
	}

	private void ComputeValues()
	{
		int num = m_Values.Length;
		float num2 = (m_To - m_From) / (float)(num - 1);
		float num3 = m_Func(m_From);
		float num4 = 0f;
		m_Values[0] = 0f;
		for (int i = 1; i < num; i++)
		{
			float arg = m_From + (float)i * num2;
			float num5 = m_Func(arg);
			num4 += num2 * (num5 + num3) / 2f;
			num3 = num5;
			m_Values[i] = num4;
		}
	}
}
