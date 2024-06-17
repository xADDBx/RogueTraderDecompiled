using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct SpatialHashmapFindPairsJob : IJobParallelFor
{
	[ReadOnly]
	public int Offset;

	[ReadOnly]
	public uint2 CollidersRange;

	[ReadOnly]
	public uint2 ForceVolumesRange;

	[ReadOnly]
	public float CellSize;

	[ReadOnly]
	public float InvCellSize;

	[ReadOnly]
	public uint HashtableSize;

	[ReadOnly]
	public uint FrameId;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> SpatialHashmapKeys;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> SpatialHashmapValues;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> SpatialHashmapFrameId;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> AabbMin;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> AabbMax;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BodyColliderPairs;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BodyForceVolumePairs;

	public void Execute(int index)
	{
		int index2 = index + Offset;
		float3 @float = AabbMin[index2];
		float3 float2 = AabbMax[index2];
		int num = 0;
		int num2 = 0;
		int3 @int = PBDMath.SpatialDiscretizePosition(@float, InvCellSize);
		int3 int2 = PBDMath.SpatialDiscretizePosition(float2, InvCellSize) + 1;
		for (int i = @int.z; i < int2.z; i++)
		{
			for (int j = @int.y; j < int2.y; j++)
			{
				for (int k = @int.x; k < int2.x; k++)
				{
					uint num3 = PBDMath.SpatialHash(new int3(k, j, i));
					uint num4 = num3 % HashtableSize;
					int num5 = 0;
					while (num5 < 100 && SpatialHashmapFrameId[(int)num4] == FrameId)
					{
						if (SpatialHashmapKeys[(int)num4] != num3)
						{
							num4 = (num4 + 1) % HashtableSize;
							num5++;
							continue;
						}
						uint num6 = SpatialHashmapValues[(int)num4];
						if (num6 >= CollidersRange.x && num6 < CollidersRange.y && num < 16)
						{
							float3 min = AabbMin[(int)num6];
							float3 max = AabbMax[(int)num6];
							if (PBDMath.TestAABBOverlap(@float, float2, min, max))
							{
								BodyColliderPairs[index * 16 + num] = (int)(num6 - CollidersRange.x);
								num++;
							}
						}
						if (num6 >= ForceVolumesRange.x && num6 < ForceVolumesRange.y && num2 < 8)
						{
							float3 min2 = AabbMin[(int)num6];
							float3 max2 = AabbMax[(int)num6];
							if (PBDMath.TestAABBOverlap(@float, float2, min2, max2))
							{
								BodyForceVolumePairs[index * 8 + num2] = (int)(num6 - ForceVolumesRange.x);
								num2++;
							}
						}
						num4 = (num4 + 1) % HashtableSize;
						num5++;
					}
				}
			}
		}
		for (int l = num; l < 16; l++)
		{
			BodyColliderPairs[index * 16 + l] = -1;
		}
		for (int m = num2; m < 8; m++)
		{
			BodyForceVolumePairs[index * 8 + m] = -1;
		}
	}
}
