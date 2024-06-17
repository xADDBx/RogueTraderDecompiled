using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.RayView;

public struct RayDescriptor
{
	public float3 Start;

	public quaternion StartRotation;

	public float3 End;

	public Space OffsetSpace;

	public float2 UvOffset;

	public float WidthScale;

	public float FadeWidth;

	public float FadeAlphaDistance;

	public float FadeAlphaSpeed;

	public float FadeUvSpeed;

	public float OffsetCurveBias;

	public RayState State;

	public void SetState(RayState state)
	{
		if (state != State)
		{
			switch (state)
			{
			case RayState.FadeIn:
				FadeAlphaDistance = 0f - FadeWidth;
				break;
			case RayState.FadeOut:
				FadeAlphaDistance = 0f - FadeWidth;
				break;
			}
			State = state;
		}
	}
}
