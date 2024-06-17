using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct SpatialHasmapLoadFactorJob : IJob
{
	public uint FrameId;

	public uint HashtableCapacity;

	public NativeArray<float> LoadFactor;

	public NativeArray<uint> SpatialHashtableFrameId;

	public void Execute()
	{
		uint num = 0u;
		for (int i = 0; i < HashtableCapacity; i++)
		{
			if (SpatialHashtableFrameId[i] == FrameId)
			{
				num++;
			}
		}
		LoadFactor[0] = (float)num / (float)HashtableCapacity;
	}
}
