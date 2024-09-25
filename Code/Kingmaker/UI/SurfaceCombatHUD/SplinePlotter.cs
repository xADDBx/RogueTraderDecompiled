using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Kingmaker.UI.SurfaceCombatHUD;

[BurstCompile]
internal struct SplinePlotter<TSplineMetaData, TListener> where TSplineMetaData : unmanaged where TListener : struct, ISplinePlotterListener<TSplineMetaData>
{
	private TListener m_Listener;

	private readonly NativeArray<BezierPoint> m_SmoothBezierPoints;

	private readonly float m_HardTurnSmoothDistanceFactor;

	private float3 m_PendingPointPosition;

	private quaternion m_PendingPointRotation;

	private float m_PendingPointSmoothDistance;

	private bool m_PendingPointBreakLine;

	private bool m_PendingPointMasked;

	private float m_SpatialDistance;

	private float3 m_SpatialDistanceLastPosition;

	private bool m_PendingBreakLine;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SplinePlotter(TListener listener, NativeArray<BezierPoint> smoothBezierPoints, float hardTurnSmoothDistanceFactor)
	{
		m_Listener = listener;
		m_SmoothBezierPoints = smoothBezierPoints;
		m_HardTurnSmoothDistanceFactor = hardTurnSmoothDistanceFactor;
		m_PendingPointPosition = default(float3);
		m_PendingPointRotation = default(quaternion);
		m_PendingPointSmoothDistance = 0f;
		m_PendingPointBreakLine = false;
		m_PendingPointMasked = false;
		m_PendingBreakLine = false;
		m_SpatialDistance = 0f;
		m_SpatialDistanceLastPosition = default(float3);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void StartLine(float3 position, quaternion rotation)
	{
		m_PendingPointPosition = position;
		m_PendingPointRotation = rotation;
		m_PendingPointSmoothDistance = 0f;
		m_PendingPointBreakLine = false;
		m_PendingPointMasked = false;
		m_PendingBreakLine = false;
		m_SpatialDistance = 0f;
		m_SpatialDistanceLastPosition = position;
		m_Listener.StartLine();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FinishLine(TSplineMetaData metaData)
	{
		AddLinePoint(in m_PendingPointPosition, in m_PendingPointRotation, breakLine: false, m_PendingPointMasked, optional: false);
		m_Listener.FinishLine(in metaData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushPoint(float3 position, float smoothDistance, float segmentedDistance, bool masked)
	{
		PushPoint(position, quaternion.LookRotationSafe(position - m_PendingPointPosition, new float3(0f, 1f, 0f)), smoothDistance, segmentedDistance, masked);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void PushPoint(float3 position, quaternion rotation, float smoothDistance, float segmentedDistance, bool masked)
	{
		if (m_PendingPointSmoothDistance > 0f)
		{
			float3 @float = math.rotate(m_PendingPointRotation, new float3(0f, 0f, -1f));
			float3 float2 = math.rotate(rotation, new float3(0f, 0f, 1f));
			float num = m_PendingPointSmoothDistance * (1f + m_HardTurnSmoothDistanceFactor * math.max(0f, math.dot(@float, float2)));
			float3 float3 = @float * num;
			float3 float4 = float2 * num;
			float3 enterPosition = m_PendingPointPosition + float3;
			float3 exitPosition = m_PendingPointPosition + float4;
			AddCurve(in enterPosition, in m_PendingPointPosition, in exitPosition, in m_PendingPointRotation, in rotation, in m_PendingPointMasked, in masked, in m_PendingPointBreakLine);
		}
		else
		{
			AddLinePoint(in m_PendingPointPosition, in m_PendingPointRotation, m_PendingPointBreakLine, m_PendingPointMasked, optional: false);
		}
		m_PendingPointPosition = position;
		m_PendingPointRotation = rotation;
		m_PendingPointSmoothDistance = smoothDistance;
		m_PendingPointMasked = masked;
		m_PendingPointBreakLine = m_PendingBreakLine;
		m_PendingBreakLine = false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void BreakLine()
	{
		m_PendingBreakLine = true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddCurve(in float3 enterPosition, in float3 controlPosition, in float3 exitPosition, in quaternion enterRotation, in quaternion exitRotation, in bool enterMasked, in bool exitMasked, in bool breakLine)
	{
		AddLinePoint(in enterPosition, in enterRotation, breakLine: false, enterMasked, optional: false);
		if (m_SmoothBezierPoints.Length > 2)
		{
			float num = 1f / (float)(m_SmoothBezierPoints.Length - 1);
			int num2 = m_SmoothBezierPoints.Length / 2;
			int i = 1;
			for (int num3 = m_SmoothBezierPoints.Length - 1; i < num3; i++)
			{
				float3 position = m_SmoothBezierPoints[i].Evaluate(in enterPosition, in controlPosition, in exitPosition);
				quaternion rotation = math.slerp(enterRotation, exitRotation, (float)i * num);
				AddLinePoint(in position, in rotation, breakLine && i == num2, (i < num2) ? enterMasked : exitMasked, optional: true);
			}
		}
		AddLinePoint(in exitPosition, in exitRotation, breakLine && m_SmoothBezierPoints.Length <= 2, exitMasked, optional: false);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void AddLinePoint(in float3 position, in quaternion rotation, bool breakLine, bool masked, bool optional)
	{
		m_SpatialDistance += math.distance(position, m_SpatialDistanceLastPosition);
		m_SpatialDistanceLastPosition = position;
		SplinePoint point = default(SplinePoint);
		point.position = position;
		point.rotation = rotation;
		point.spatialDistance = m_SpatialDistance;
		point.segmentedDistance = 0f;
		point.breakLine = breakLine;
		point.masked = masked;
		point.optional = optional;
		m_Listener.PushPoint(in point);
	}
}
