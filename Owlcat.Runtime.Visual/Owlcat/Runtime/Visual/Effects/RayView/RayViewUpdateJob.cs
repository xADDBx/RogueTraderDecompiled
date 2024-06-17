using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.Effects.LineRenderer;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects.RayView;

[BurstCompile]
public struct RayViewUpdateJob : IJobLineRenderer, IJobParallelFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	private NativeArray<Point> m_Points;

	private NativeArray<LineDescriptor> m_Lines;

	public NativeArray<RayDescriptor> RayDescriptors;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public JobAnimationCurve PlaneOffsetMainCurve;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public JobCompositeAnimationCurve PlaneOffsetAdditionalCurve;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public JobAnimationCurve UpOffsetMainCurve;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public JobCompositeAnimationCurve UpOffsetAdditionalCurve;

	public float GlobalTime;

	public float DeltaTime;

	public NativeArray<Point> Points
	{
		get
		{
			return m_Points;
		}
		set
		{
			m_Points = value;
		}
	}

	public NativeArray<LineDescriptor> Lines
	{
		get
		{
			return m_Lines;
		}
		set
		{
			m_Lines = value;
		}
	}

	public void Execute(int index)
	{
		LineDescriptor value = m_Lines[index];
		if (value.PositionCount < 2)
		{
			return;
		}
		RayDescriptor value2 = RayDescriptors[index];
		float3 @float = value2.End - value2.Start;
		float num = math.length(@float);
		@float /= num;
		switch (value2.State)
		{
		case RayState.FadeIn:
			value2.UvOffset -= new float2(value2.FadeUvSpeed * DeltaTime, 0f);
			value2.FadeAlphaDistance += value2.FadeAlphaSpeed * DeltaTime;
			if (value2.FadeAlphaDistance >= num)
			{
				value2.SetState(RayState.AfterHit);
			}
			break;
		case RayState.FadeOut:
			value2.UvOffset -= new float2(value2.FadeUvSpeed * DeltaTime, 0f);
			value2.FadeAlphaDistance += value2.FadeAlphaSpeed * DeltaTime;
			if (value2.FadeAlphaDistance >= num)
			{
				value2.SetState(RayState.Finished);
			}
			break;
		}
		RayDescriptors[index] = value2;
		value.UvOffset = value2.UvOffset;
		value.WidthScale = value2.WidthScale;
		m_Lines[index] = value;
		for (int i = 0; i < value.PositionCount; i++)
		{
			float num2 = (float)i / (float)(value.PositionCount - 1);
			float x = PlaneOffsetMainCurve.EvaluateNormalized(num2) + PlaneOffsetAdditionalCurve.EvaluateNormalized(num2, GlobalTime + value2.OffsetCurveBias);
			float y = UpOffsetMainCurve.EvaluateNormalized(num2) + UpOffsetAdditionalCurve.EvaluateNormalized(num2, GlobalTime + value2.OffsetCurveBias);
			float3 float2 = new float3(x, y, 0f);
			if (value2.OffsetSpace == Space.Self)
			{
				float2 = math.rotate(value2.StartRotation, float2);
			}
			float num3 = num2 * num;
			float alpha = 1f;
			switch (value2.State)
			{
			case RayState.FadeIn:
				alpha = math.lerp(1f, 0f, math.saturate((num3 - value2.FadeAlphaDistance) / value2.FadeWidth));
				break;
			case RayState.Normal:
				alpha = 1f;
				break;
			case RayState.FadeOut:
				alpha = math.lerp(0f, 1f, math.saturate((num3 - value2.FadeAlphaDistance) / value2.FadeWidth));
				break;
			case RayState.AfterHit:
				alpha = 1f;
				break;
			case RayState.Finished:
				alpha = 0f;
				break;
			}
			float3 float3 = value2.Start + @float * num3;
			m_Points[value.PositionsOffset + i] = new Point
			{
				Position = float3 + float2,
				Alpha = alpha
			};
		}
	}
}
