using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class FloatAnimationClip : IAnimationClip
{
	private readonly PropertyIdentifier m_PropertyIdentifier;

	private readonly AnimationCurve m_Curve;

	private readonly float m_CurveDuration;

	public FloatAnimationClip(string propertyName, AnimationCurve curve, float curveDuration)
	{
		m_PropertyIdentifier = new PropertyIdentifier(propertyName);
		m_Curve = curve;
		m_CurveDuration = curveDuration;
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		float value = m_Curve.Evaluate(time / m_CurveDuration);
		properties.SetFloat(m_PropertyIdentifier, value);
	}

	void IAnimationClip.Sample(in PropertyBlock properties, float time)
	{
		Sample(in properties, time);
	}
}
