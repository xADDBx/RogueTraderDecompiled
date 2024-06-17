using System;
using UnityEngine;

namespace Kingmaker.Visual.Decals;

[Serializable]
public class FxDecalAnimationSettings
{
	[SerializeField]
	[HideInInspector]
	internal bool IsInitialized;

	public float Lifetime;

	[Header("Scale Over Lifetime")]
	public AnimationCurve ScaleXZ = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve ScaleX = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve ScaleY = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	public AnimationCurve ScaleZ = AnimationCurve.Linear(0f, 1f, 1f, 1f);

	[Header("Colors")]
	public Gradient AlbedoColorOverLifetime = new Gradient();

	public Gradient EmissionColorOverLifetime = new Gradient();

	public AnimationCurve SubstractAlphaOverLifetime = AnimationCurve.Linear(0f, 0f, 1f, 0f);

	public bool LoopAnimation;

	internal float CurrentTime;

	internal Vector3 CurrentScale;

	internal Color CurrentAlbedoColor;

	internal Color CurrentEmissionColor;

	internal float CurrentSubstractAlpha;

	public void Reset()
	{
		CurrentTime = 0f;
	}
}
