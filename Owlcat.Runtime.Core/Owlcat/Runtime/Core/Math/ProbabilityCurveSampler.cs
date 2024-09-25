using UnityEngine;

namespace Owlcat.Runtime.Core.Math;

public class ProbabilityCurveSampler
{
	private readonly AnimationCurve m_DensityCurve;

	private readonly IntegrateFunc m_IntegratedDensity;

	private float MinT => m_DensityCurve.keys[0].time;

	private float MaxT => m_DensityCurve.keys[m_DensityCurve.length - 1].time;

	public ProbabilityCurveSampler(AnimationCurve curve, int integrationSteps = 100)
	{
		m_DensityCurve = curve;
		m_IntegratedDensity = new IntegrateFunc(curve.Evaluate, curve.keys[0].time, curve.keys[curve.length - 1].time, integrationSteps);
	}

	public float Sample(float unitValue)
	{
		return Invert(unitValue);
	}

	public float SampleRandom()
	{
		return Invert(Random.value);
	}

	private float Invert(float s)
	{
		s *= m_IntegratedDensity.Total;
		float num = MinT;
		float num2 = MaxT;
		while (num2 - num > 1E-05f)
		{
			float num3 = (num + num2) / 2f;
			float num4 = m_IntegratedDensity.Evaluate(num3);
			if (num4 > s)
			{
				num2 = num3;
				continue;
			}
			if (num4 < s)
			{
				num = num3;
				continue;
			}
			return num3;
		}
		return (num + num2) / 2f;
	}
}
