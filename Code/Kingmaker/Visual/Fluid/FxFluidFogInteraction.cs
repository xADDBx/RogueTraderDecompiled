using System;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Visual.Fluid;

[Obsolete]
public class FxFluidFogInteraction : MonoBehaviour
{
	private float m_Time;

	private float m_Size;

	private float m_Force;

	private float m_Dist;

	private Vector3 m_LastPos;

	private float m_RateOverDurationAccumulator;

	private float m_MaxForce = float.MinValue;

	public float Delay;

	public float Duration;

	public float Lifetime = 1f;

	[Space]
	public float RateOverDistance = 1f;

	public AnimationCurve RateOverDuration = AnimationCurve.Constant(0f, 1f, 1f);

	public float RateOverDurationMultiplier = 1f;

	[Space]
	[Range(0f, 1f)]
	public float RadialWeight = 1f;

	public bool AlwaysUseObjectRotation;

	public float RandomizePositionRadius;

	[MinMaxSlider(-180f, 180f)]
	public Vector2 RandomizeRotation;

	[Space]
	public float SizeMultiplier = 1f;

	public AnimationCurve SizeOverDuration = AnimationCurve.Constant(0f, 1f, 1f);

	[Space]
	public float ForceMultiplier = 1f;

	public AnimationCurve ForceOverDuration = AnimationCurve.Constant(0f, 1f, 1f);
}
