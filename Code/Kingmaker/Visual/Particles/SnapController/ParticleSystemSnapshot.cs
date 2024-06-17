using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal sealed class ParticleSystemSnapshot
{
	public struct BurstSnapshot
	{
		public readonly short minCount;

		public readonly short maxCount;

		public BurstSnapshot(ParticleSystem.Burst burst)
		{
			minCount = burst.minCount;
			maxCount = burst.maxCount;
		}
	}

	public struct MinMaxCurveSnapshot
	{
		private readonly ParticleSystemCurveMode mode;

		private readonly float constant;

		private readonly float constantMin;

		private readonly float constantMax;

		private readonly float curveMultiplier;

		public MinMaxCurveSnapshot(ParticleSystem.MinMaxCurve minMaxCurve)
		{
			mode = minMaxCurve.mode;
			constant = minMaxCurve.constant;
			constantMin = minMaxCurve.constantMin;
			constantMax = minMaxCurve.constantMax;
			curveMultiplier = minMaxCurve.curveMultiplier;
		}

		public void ApplyTo(ref ParticleSystem.MinMaxCurve minMaxCurve, float scale)
		{
			minMaxCurve.mode = mode;
			minMaxCurve.constant = constant * scale;
			minMaxCurve.constantMin = constantMin * scale;
			minMaxCurve.constantMax = constantMax * scale;
			minMaxCurve.curveMultiplier = curveMultiplier * scale;
		}

		public float Evaluate(in ParticleSystem.MinMaxCurve minMaxCurveSource, float lerpFactor)
		{
			switch (mode)
			{
			case ParticleSystemCurveMode.Constant:
				return constant;
			case ParticleSystemCurveMode.TwoConstants:
				return Mathf.Lerp(constantMin, constantMax, lerpFactor);
			case ParticleSystemCurveMode.Curve:
				if (minMaxCurveSource.mode != ParticleSystemCurveMode.Curve)
				{
					return curveMultiplier;
				}
				return minMaxCurveSource.curve.Evaluate(0f) * curveMultiplier;
			case ParticleSystemCurveMode.TwoCurves:
				if (minMaxCurveSource.mode != ParticleSystemCurveMode.TwoCurves)
				{
					return curveMultiplier;
				}
				return Mathf.Lerp(minMaxCurveSource.curveMin.Evaluate(0f), minMaxCurveSource.curveMax.Evaluate(0f), lerpFactor) * curveMultiplier;
			default:
				return constant;
			}
		}
	}

	private static readonly Stack<ParticleSystemSnapshot> s_Pool = new Stack<ParticleSystemSnapshot>();

	public MinMaxCurveSnapshot startDelay;

	public MinMaxCurveSnapshot startSize;

	public MinMaxCurveSnapshot startSizeX;

	public MinMaxCurveSnapshot startSizeY;

	public MinMaxCurveSnapshot startSizeZ;

	public MinMaxCurveSnapshot startLifetime;

	public MinMaxCurveSnapshot rateOverTime;

	public readonly List<BurstSnapshot> bursts = new List<BurstSnapshot>(2);

	public static ParticleSystemSnapshot GetPooled(ParticleSystem particleSystem)
	{
		if (!s_Pool.TryPop(out var result))
		{
			result = new ParticleSystemSnapshot();
		}
		ParticleSystem.MainModule main = particleSystem.main;
		ParticleSystem.EmissionModule emission = particleSystem.emission;
		result.startDelay = new MinMaxCurveSnapshot(main.startDelay);
		result.startSize = new MinMaxCurveSnapshot(main.startSize);
		result.startSizeX = new MinMaxCurveSnapshot(main.startSizeX);
		result.startSizeY = new MinMaxCurveSnapshot(main.startSizeY);
		result.startSizeZ = new MinMaxCurveSnapshot(main.startSizeZ);
		result.startLifetime = new MinMaxCurveSnapshot(main.startLifetime);
		result.rateOverTime = new MinMaxCurveSnapshot(emission.rateOverTime);
		int i = 0;
		for (int burstCount = emission.burstCount; i < burstCount; i++)
		{
			result.bursts.Add(new BurstSnapshot(emission.GetBurst(i)));
		}
		return result;
	}

	private ParticleSystemSnapshot()
	{
	}

	public void Release()
	{
		startDelay = default(MinMaxCurveSnapshot);
		startSize = default(MinMaxCurveSnapshot);
		startSizeX = default(MinMaxCurveSnapshot);
		startSizeY = default(MinMaxCurveSnapshot);
		startSizeZ = default(MinMaxCurveSnapshot);
		startLifetime = default(MinMaxCurveSnapshot);
		rateOverTime = default(MinMaxCurveSnapshot);
		bursts.Clear();
		s_Pool.Push(this);
	}
}
