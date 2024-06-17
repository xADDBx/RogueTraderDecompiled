using System;
using UnityEngine;

namespace Kingmaker.Visual.MaterialEffects.Dissolve;

[Serializable]
public class DissolveSettings
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
	public AnimationCurve DissolveOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 1f);

	public AnimationCurve DissolveWidthOverLifetime = AnimationCurve.Linear(0f, 0.1f, 1f, 0.1f);

	public float DissolveWidthScale = 1f;

	public Gradient ColorOverLifetime = new Gradient();

	public float HdrColorScale = 1f;

	public bool LoopAnimation;

	[Header("Misc")]
	public bool DissolveCutout = true;

	public bool DissolveEmission = true;

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
	public Color CurrentColor;

	[NonSerialized]
	public float CurrentDissolve;

	[NonSerialized]
	public float CurrentDissolveWidth;

	[NonSerialized]
	public Vector2 CurrentTextureOffset;

	[NonSerialized]
	public float FadeOut = 1f;

	public bool PlayBackwards { get; set; }

	public bool UseUnscaledTime { get; set; }

	public bool IsDelayed => NormalizedTime < 0f;

	public void Reset()
	{
		IsStarted = false;
		IsFinished = false;
		NormalizedTime = 0f;
		StartTime = 0f;
		CurrentColor = Color.black;
		CurrentDissolve = 0f;
		CurrentDissolveWidth = 0f;
		CurrentTextureOffset = Vector2.zero;
		FadeOut = 1f;
	}
}
