using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct ClearSpatialHashmapJob : IJobParallelFor
{
	[WriteOnly]
	public NativeArray<uint> SpatialHashFrameId;

	[WriteOnly]
	public NativeArray<int> BodyColliderPairs;

	[WriteOnly]
	public NativeArray<int> BodyForceVolumePairs;

	public void Execute(int index)
	{
		SpatialHashFrameId[index] = uint.MaxValue;
		if (index < BodyColliderPairs.Length)
		{
			BodyColliderPairs[index] = -1;
		}
		if (index < BodyForceVolumePairs.Length)
		{
			BodyForceVolumePairs[index] = -1;
		}
	}
}
