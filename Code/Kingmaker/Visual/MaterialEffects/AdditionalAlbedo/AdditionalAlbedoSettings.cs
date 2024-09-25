using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Visual.MaterialEffects.AdditionalAlbedo;

[Serializable]
public class AdditionalAlbedoSettings
{
	[Header("Texture Settings")]
	public Texture2D Texture;

	public Vector2 TilingScale = Vector2.one;

	public bool TilingOverride = true;

	public Vector2 OffsetSpeed = Vector2.zero;

	[Header("Timing")]
	public float Delay;

	public float Lifetime = 1f;

	[Header("Animations")]
	[FormerlySerializedAs("PetrificationOverLifetime")]
	public AnimationCurve FactorOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public bool LoopAnimation;

	[Header("Visual")]
	public Color Color = Color.yellow;

	public float ColorScale = 1f;

	public AnimationCurve AlphaScaleOverLifetime = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[NonSerialized]
	public float StartTime;

	[NonSerialized]
	public bool IsStarted;

	[NonSerialized]
	public float NormalizedTime;

	[NonSerialized]
	public bool IsFinished;

	[NonSerialized]
	public Vector2 CurrentTextureOffset;

	[NonSerialized]
	public float CurrentPetrification;

	[NonSerialized]
	public float CurrentAlphaScale;

	[NonSerialized]
	public float FadeOut = 1f;

	public bool IsDelayed => NormalizedTime < 0f;

	public void Reset()
	{
		IsStarted = false;
		IsFinished = false;
		NormalizedTime = 0f;
		StartTime = 0f;
		CurrentPetrification = 0f;
		CurrentAlphaScale = 1f;
		CurrentTextureOffset = Vector2.zero;
		FadeOut = 1f;
	}
}
