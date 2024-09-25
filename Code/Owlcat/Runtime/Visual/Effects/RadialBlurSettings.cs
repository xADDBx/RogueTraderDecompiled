using System;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects;

[Serializable]
public class RadialBlurSettings
{
	[Header("Timing")]
	public float Delay;

	public float Lifetime = 1f;

	[Header("Animations")]
	public AnimationCurve StrengthOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve WidthOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[Space]
	public AnimationCurve StrengthOverDistance = AnimationCurve.Linear(0f, 1f, 1f, 0f);

	public AnimationCurve WidthOverDistance = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public float StrengthMultiplier = 4f;

	public float WidthMultiplier = 0.2f;

	public bool LoopAnimation;

	public int Layer;

	[NonSerialized]
	public float StartTime;

	[NonSerialized]
	public bool IsStarted;

	[NonSerialized]
	public float NormalizedTime;

	[NonSerialized]
	public bool IsFinished;

	[NonSerialized]
	public Vector3 TargetPosition;

	[NonSerialized]
	public Transform TargetObject;

	[NonSerialized]
	public float CurrentStrength;

	[NonSerialized]
	public float CurrentWidth;

	[NonSerialized]
	public Vector2 CurrentCenter;

	public bool IsDelayed => NormalizedTime < 0f;

	internal void Reset()
	{
		NormalizedTime = 0f;
		IsFinished = false;
		IsStarted = false;
	}
}
