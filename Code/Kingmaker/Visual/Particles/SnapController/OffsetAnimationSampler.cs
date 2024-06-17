using UnityEngine;

namespace Kingmaker.Visual.Particles.SnapController;

internal readonly struct OffsetAnimationSampler
{
	private readonly bool m_OffsetAnimationEnabled;

	private readonly Transform m_OffsetRotationRoot;

	private readonly AnimationCurve m_OffsetCurveX;

	private readonly AnimationCurve m_OffsetCurveY;

	private readonly AnimationCurve m_OffsetCurveZ;

	private readonly bool m_OffsetUseWorldRotation;

	private readonly float m_ParticleSystemStartDelay;

	public OffsetAnimationSampler(float particleSystemStartDelay, in SnapControllerBase.OffsetAnimationSettings offsetAnimationSettings, Transform offsetRotationRoot)
	{
		m_ParticleSystemStartDelay = particleSystemStartDelay;
		m_OffsetRotationRoot = offsetRotationRoot;
		m_OffsetAnimationEnabled = offsetAnimationSettings.Enabled;
		m_OffsetCurveX = offsetAnimationSettings.OffsetX;
		m_OffsetCurveY = offsetAnimationSettings.OffsetY;
		m_OffsetCurveZ = offsetAnimationSettings.OffsetZ;
		m_OffsetUseWorldRotation = offsetAnimationSettings.UseWorldRotation;
	}

	public Vector3 Sample(float particleSystemTime)
	{
		if (!m_OffsetAnimationEnabled)
		{
			return default(Vector3);
		}
		float time = Mathf.Max(0f, particleSystemTime - m_ParticleSystemStartDelay);
		Vector3 vector = new Vector3(m_OffsetCurveX.Evaluate(time), m_OffsetCurveY.Evaluate(time), m_OffsetCurveZ.Evaluate(time));
		if (m_OffsetRotationRoot != null)
		{
			vector.Scale(m_OffsetRotationRoot.lossyScale);
			if (m_OffsetUseWorldRotation)
			{
				return m_OffsetRotationRoot.TransformDirection(vector);
			}
		}
		return vector;
	}
}
