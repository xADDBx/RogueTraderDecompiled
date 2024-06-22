using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Kingmaker.Controllers.FogOfWar.Culling;

[BurstCompile]
internal struct BuildBlockerPlaneSetsJob : IJobParallelFor
{
	[ReadOnly]
	public NativeArray<RevealerProperties> Revealers;

	[ReadOnly]
	public NativeArray<BlockerSegment> BlockerSegments;

	[ReadOnly]
	public NativeArray<float4> BlockerPlanes;

	[WriteOnly]
	public NativeArray<int> BlockerPlaneSetCounts;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<BlockerPlaneSet> BlockerPlaneSets;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void Execute(int revealerIndex)
	{
		RevealerProperties revealerProperties = Revealers[revealerIndex];
		int length = BlockerSegments.Length;
		NativeSlice<BlockerPlaneSet> nativeSlice = BlockerPlaneSets.Slice(revealerIndex * length, length);
		int num = 0;
		for (int i = 0; i < length; i++)
		{
			BlockerSegment blockerSegment = BlockerSegments[i];
			float4 plane = BlockerPlanes[i];
			float num2 = 0f - Plane2D.SignedDistance(in plane, in revealerProperties.Center);
			if (!(num2 <= 0f) && !(num2 >= revealerProperties.Radius) && (!(revealerProperties.HeightMin > blockerSegment.HeightMax) || !(revealerProperties.HeightMax > blockerSegment.HeightMax)) && (!(revealerProperties.HeightMin < blockerSegment.HeightMin) || !(revealerProperties.HeightMax < blockerSegment.HeightMin)))
			{
				nativeSlice[num] = new BlockerPlaneSet
				{
					PlaneA = plane,
					PlaneB = Plane2D.PlaneFromTwoPoints(in blockerSegment.PointA, in revealerProperties.Center),
					PlaneC = Plane2D.PlaneFromTwoPoints(in revealerProperties.Center, in blockerSegment.PointB)
				};
				num++;
			}
		}
		BlockerPlaneSetCounts[revealerIndex] = num;
	}
}
