using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class ColorAnimationClip : IAnimationClip
{
	private static readonly Func<float, float> s_ClampWrapFunc = (float time) => Mathf.Clamp01(time);

	private static readonly Func<float, float> s_LoopWrapFunc = (float time) => Mathf.Repeat(time, 1f);

	private static readonly Func<float, float> s_PingPongWrapFunc = (float time) => Mathf.PingPong(time, 1f);

	private readonly PropertyIdentifier m_PropertyIdentifier;

	private readonly Gradient m_Gradient;

	private readonly float m_GradientDuration;

	private readonly Func<float, float> m_WrapFunc;

	public ColorAnimationClip(string propertyName, Gradient gradient, float gradientDuration, GradientWrapMode gradientWrapMode)
	{
		m_PropertyIdentifier = new PropertyIdentifier(propertyName);
		m_Gradient = gradient;
		m_GradientDuration = gradientDuration;
		switch (gradientWrapMode)
		{
		case GradientWrapMode.Loop:
			m_WrapFunc = s_LoopWrapFunc;
			break;
		case GradientWrapMode.PingPong:
			m_WrapFunc = s_PingPongWrapFunc;
			break;
		default:
			m_WrapFunc = s_ClampWrapFunc;
			break;
		}
	}

	public void Sample(in PropertyBlock properties, float time)
	{
		float time2 = m_WrapFunc(time / m_GradientDuration);
		Color value = m_Gradient.Evaluate(time2);
		properties.SetColor(m_PropertyIdentifier, value);
	}

	void IAnimationClip.Sample(in PropertyBlock properties, float time)
	{
		Sample(in properties, time);
	}
}
