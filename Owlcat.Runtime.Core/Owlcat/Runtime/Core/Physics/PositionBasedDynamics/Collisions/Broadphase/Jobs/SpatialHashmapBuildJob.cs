using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct SpatialHashmapBuildJob : IJob
{
	public int Count;

	public float CellSize;

	public float InvCellSize;

	public uint HashtableSize;

	public uint FrameId;

	public NativeArray<uint> SpatialHashtableKeys;

	public NativeArray<uint> SpatialHashmapValues;

	public NativeArray<uint> SpatialHashmapFrameId;

	public NativeArray<float3> AabbMin;

	public NativeArray<float3> AabbMax;

	public void Execute()
	{
		for (int i = 0; i < Count; i++)
		{
			float3 position = AabbMin[i];
			float3 position2 = AabbMax[i];
			int3 @int = PBDMath.SpatialDiscretizePosition(position, InvCellSize);
			int3 int2 = PBDMath.SpatialDiscretizePosition(position2, InvCellSize) + 1;
			for (int j = @int.z; j < int2.z; j++)
			{
				for (int k = @int.y; k < int2.y; k++)
				{
					for (int l = @int.x; l < int2.x; l++)
					{
						uint num = PBDMath.SpatialHash(new int3(l, k, j));
						uint num2 = num % HashtableSize;
						int num3 = 0;
						while (num3 < 100)
						{
							if (SpatialHashmapFrameId[(int)num2] != FrameId)
							{
								SpatialHashtableKeys[(int)num2] = num;
								SpatialHashmapValues[(int)num2] = (uint)i;
								SpatialHashmapFrameId[(int)num2] = FrameId;
								break;
							}
							num3++;
							num2 = (num2 + 1) % HashtableSize;
						}
					}
				}
			}
		}
	}
}
