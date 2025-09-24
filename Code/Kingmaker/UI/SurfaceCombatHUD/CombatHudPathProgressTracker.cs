using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Kingmaker.UI.SurfaceCombatHUD;

internal sealed class CombatHudPathProgressTracker : IDisposable
{
	[BurstCompile]
	internal struct UpdateProgressJob : IJob
	{
		public struct Result
		{
			public float TraveledDistance;

			public float3 Position;
		}

		[ReadOnly]
		public NativeList<ApproximatePathSegment> Segments;

		[ReadOnly]
		public float3 TargetPosition;

		[WriteOnly]
		public NativeReference<Result> OutResult;

		public void Execute()
		{
			if (Segments.Length < 2)
			{
				OutResult.Value = new Result
				{
					TraveledDistance = 0f,
					Position = default(float3)
				};
				return;
			}
			float num = float.PositiveInfinity;
			float3 position = float3.zero;
			float traveledDistance = 0f;
			float3 segmentOrigin = Segments[0].direction;
			float start = 0f;
			for (int i = 1; i < Segments.Length; i++)
			{
				ApproximatePathSegment segment = Segments[i];
				GetSegmentInfo(in segment, in segmentOrigin, out var position2, out var dot, out var distanceSqToTarget);
				if (distanceSqToTarget <= num)
				{
					num = distanceSqToTarget;
					position = position2;
					traveledDistance = math.lerp(start, segment.spatialDistanceAtEnd, dot / segment.length);
				}
				segmentOrigin += segment.direction * segment.length;
				start = segment.spatialDistanceAtEnd;
			}
			OutResult.Value = new Result
			{
				TraveledDistance = traveledDistance,
				Position = position
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void GetSegmentInfo(in ApproximatePathSegment segment, in float3 segmentOrigin, out float3 position, out float dot, out float distanceSqToTarget)
		{
			dot = math.clamp(math.dot(TargetPosition - segmentOrigin, segment.direction), 0f, segment.length);
			position = segmentOrigin + segment.direction * dot;
			distanceSqToTarget = math.distancesq(TargetPosition, position);
		}
	}

	private NativeList<ApproximatePathSegment> m_Segments;

	private NativeReference<UpdateProgressJob.Result> m_Result;

	private Transform m_TargetTransform;

	private Vector3 m_TargetPosition;

	private bool m_IsProgressValid;

	private Renderer m_Renderer;

	public CombatHudPathProgressTracker(Allocator allocator)
	{
		m_Segments = new NativeList<ApproximatePathSegment>(allocator);
		m_Result = new NativeReference<UpdateProgressJob.Result>(allocator);
	}

	public void Dispose()
	{
		m_Segments.Dispose();
		m_Result.Dispose();
	}

	public float GetTraveledDistance()
	{
		return m_Result.Value.TraveledDistance;
	}

	public float3 GetTraveledPosition()
	{
		return m_Result.Value.Position;
	}

	public NativeList<ApproximatePathSegment> GetPath()
	{
		return m_Segments;
	}

	public void SetTargetTransform(Transform value)
	{
		m_TargetTransform = value;
	}

	public void Invalidate()
	{
		m_IsProgressValid = false;
	}

	public JobHandle ScheduleJobs(JobHandle dependsOn)
	{
		if (m_TargetTransform == null)
		{
			m_IsProgressValid = true;
			m_Result.Value = default(UpdateProgressJob.Result);
			return dependsOn;
		}
		Vector3 position = m_TargetTransform.position;
		if (m_IsProgressValid && m_TargetPosition == position)
		{
			m_IsProgressValid = true;
			return dependsOn;
		}
		m_IsProgressValid = true;
		UpdateProgressJob jobData = default(UpdateProgressJob);
		jobData.Segments = m_Segments;
		jobData.TargetPosition = position;
		jobData.OutResult = m_Result;
		return jobData.Schedule(dependsOn);
	}
}
