using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.FogOfWar.Culling;

[BurstCompile]
internal struct BuildBlockerPlanesJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<BlockerSegment> BlockerSegments;

	[WriteOnly]
	public NativeArray<float4> BlockerPlanes;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Execute(int blockerIndex)
	{
		BlockerSegment blockerSegment = BlockerSegments[blockerIndex];
		BlockerPlanes[blockerIndex] = Plane2D.PlaneFromTwoPoints(in blockerSegment.PointA, in blockerSegment.PointB);
	}
}
