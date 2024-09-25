using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct CameraOffsetScaleAnimationSampler
{
	private readonly float m_ParticleSystemStartDelay;

	private readonly AnimationCurve m_CameraOffsetScaleCurve;

	private readonly float m_CameraOffsetScaleFactor;

	public CameraOffsetScaleAnimationSampler(float particleSystemStartDelay, AnimationCurve cameraOffsetScaleCurve, float snapMapAdditionalScaleReduced)
	{
		m_ParticleSystemStartDelay = particleSystemStartDelay;
		m_CameraOffsetScaleCurve = cameraOffsetScaleCurve;
		m_CameraOffsetScaleFactor = snapMapAdditionalScaleReduced;
	}

	public float Sample(float particleSystemTime)
	{
		float time = Mathf.Max(0f, particleSystemTime - m_ParticleSystemStartDelay);
		return m_CameraOffsetScaleCurve.Evaluate(time) * m_CameraOffsetScaleFactor;
	}
}
