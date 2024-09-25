using System.Runtime.CompilerServices;
using Owlcat.Runtime.Visual.OcclusionGeometryClip.BIH;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
internal struct FrustumCastGeometry : IHierarchyCastGeometry<OccluderGeometry>
{
	public readonly FrustumSatGeometry frustumSatGeometry;

	public readonly FrustumSatGeometry frustumSatGeometryDynamic;

	public readonly float3 viewPosition;

	public readonly float3 targetPosition;

	public readonly bool targetInsideBoxOccluded;

	public readonly Settings.DynamicTargetMode dynamicTargetMode;

	public readonly float dynamicDistanceMin;

	public readonly float dynamicDistanceDelta;

	public FrustumCastGeometry(float4x4 viewMatrix, float4x4 viewMatrixInverse, float3 targetRootPosition, float2 targetSize, bool targetInsideBoxOccluded, Settings.DynamicTargetMode dynamicTargetMode, float2 dynamicTargetSize, float dynamicDistanceMin, float dynamicDistanceMax)
	{
		frustumSatGeometry = new FrustumSatGeometry(viewMatrix, viewMatrixInverse, targetRootPosition, targetSize);
		this.targetInsideBoxOccluded = targetInsideBoxOccluded;
		this.dynamicTargetMode = dynamicTargetMode;
		this.dynamicDistanceMin = dynamicDistanceMin;
		dynamicDistanceDelta = dynamicDistanceMax - dynamicDistanceMin;
		if (dynamicTargetMode != 0)
		{
			frustumSatGeometryDynamic = new FrustumSatGeometry(viewMatrix, viewMatrixInverse, targetRootPosition, dynamicTargetSize);
		}
		else
		{
			frustumSatGeometryDynamic = default(FrustumSatGeometry);
		}
		viewPosition = frustumSatGeometry.point0;
		targetPosition = targetRootPosition;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ABox GetBounds()
	{
		return frustumSatGeometry.bounds;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IntersectsNode(ABox bounds)
	{
		return GeometryMath.Intersects(in frustumSatGeometry.frustum, in bounds);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool IntersectsLeaf(OccluderGeometry geo)
	{
		if (!GeometryMath.Intersects(in frustumSatGeometry.frustum, in geo.aBox))
		{
			return false;
		}
		if (dynamicTargetMode != 0)
		{
			return IntersectsLeafDynamic(geo);
		}
		return IntersectsLeafStatic(geo);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IntersectsLeafDynamic(OccluderGeometry geo)
	{
		GeometryMath.TestTargetAgainstBox(in geo.oBox, viewPosition, targetPosition, dynamicTargetMode == Settings.DynamicTargetMode.ProjectedToAxesDistance, out var result, out var backSideDistance);
		switch (result)
		{
		case GeometryMath.TargetPositionRelativeToBox.Inside:
			return targetInsideBoxOccluded;
		case GeometryMath.TargetPositionRelativeToBox.OnFrontSide:
			return false;
		default:
		{
			float t = math.saturate((backSideDistance - dynamicDistanceMin) / dynamicDistanceDelta);
			return GeometryMath.SatIntersects(FrustumSatGeometry.Lerp(frustumSatGeometryDynamic, frustumSatGeometry, t), new OBoxSatGeometry(in geo.oBox));
		}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool IntersectsLeafStatic(OccluderGeometry geo)
	{
		return GeometryMath.GetTargetPositionRelativeToBox(in geo.oBox, viewPosition, targetPosition) switch
		{
			GeometryMath.TargetPositionRelativeToBox.Inside => targetInsideBoxOccluded, 
			GeometryMath.TargetPositionRelativeToBox.OnFrontSide => false, 
			_ => GeometryMath.SatIntersects(frustumSatGeometry, new OBoxSatGeometry(in geo.oBox)), 
		};
	}
}
