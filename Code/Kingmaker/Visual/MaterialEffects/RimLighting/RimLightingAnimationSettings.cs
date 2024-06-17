using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.RimLighting;

[Serializable]
public class RimLightingAnimationSettings
{
	[Header("Timing")]
	public float Delay;

	public float Lifetime;

	[Header("Animations")]
	public Gradient ColorOverLifetime;

	public AnimationCurve IntensityOverLifetime;

	public float IntensityScale;

	public AnimationCurve PowerOverLifetime;

	public bool LoopAnimation;

	public bool UnscaledTime;

	[NonSerialized]
	public float StartTime;

	[NonSerialized]
	public bool IsStarted;

	[NonSerialized]
	public float NormalizedTime;

	[NonSerialized]
	public bool IsFinished;

	[NonSerialized]
	public Color CurrentColor;

	[NonSerialized]
	public float CurrentIntensity;

	[NonSerialized]
	public float CurrentPower;

	[NonSerialized]
	public float FadeOut = 1f;

	public bool IsDelayed => NormalizedTime < 0f;

	public void Reset()
	{
		IsStarted = false;
		IsFinished = false;
		NormalizedTime = 0f;
		StartTime = 0f;
		CurrentColor = Color.black;
		CurrentIntensity = 0f;
		CurrentPower = 0f;
		FadeOut = 1f;
	}
}
