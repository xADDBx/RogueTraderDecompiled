using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.ColorTint;

[Serializable]
public class ColorTintAnimationSettings
{
	[Header("Timing")]
	public float Delay;

	public float Lifetime = 1f;

	[Header("Animations")]
	public Gradient ColorOverLifetime = new Gradient();

	public bool LoopAnimation;

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
	public float FadeOut = 1f;

	public bool IsDelayed => NormalizedTime < 0f;

	public void Reset()
	{
		IsStarted = false;
		IsFinished = false;
		NormalizedTime = 0f;
		StartTime = 0f;
		CurrentColor = Color.black;
		FadeOut = 1f;
	}
}
