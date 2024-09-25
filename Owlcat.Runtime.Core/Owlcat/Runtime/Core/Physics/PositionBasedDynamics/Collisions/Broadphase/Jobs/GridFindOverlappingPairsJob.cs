using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase.Jobs;

[BurstCompile]
public struct GridFindOverlappingPairsJob : IJobParallelFor
{
	[ReadOnly]
	public int Offset;

	[ReadOnly]
	public uint2 CollidersRange;

	[ReadOnly]
	public uint2 ForceVolumesRange;

	[ReadOnly]
	public float3 CellSize;

	[ReadOnly]
	public NativeArray<float3> SceneAabb;

	[ReadOnly]
	public int HashCount;

	[ReadOnly]
	public MultilevelGridDimension Dimension;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeHashMap<uint, uint> CellStart;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<KeyValuePairComparable<uint, uint>> Hash;

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
		float3 @float = SceneAabb[0];
		int index2 = index + Offset;
		float3 float2 = AabbMin[index2];
		float3 float3 = AabbMax[index2];
		uint4 gridPos = 0u;
		int num = 0;
		int num2 = 0;
		for (uint num3 = 0u; num3 < 4; num3++)
		{
			uint4 @uint = PBDMath.MultilevelGridPos(float2 - @float, num3, CellSize);
			uint4 uint2 = PBDMath.MultilevelGridPos(float3 - @float, num3, CellSize) - @uint + 1u;
			if (Dimension == MultilevelGridDimension.Grid2D)
			{
				uint2.y = 1u;
			}
			gridPos.w = num3;
			for (uint num4 = 0u; num4 < uint2.z; num4++)
			{
				gridPos.z = @uint.z + num4;
				for (uint num5 = 0u; num5 < uint2.y; num5++)
				{
					if (Dimension == MultilevelGridDimension.Grid2D)
					{
						gridPos.y = 0u;
					}
					else
					{
						gridPos.y = @uint.y + num5;
					}
					for (uint num6 = 0u; num6 < uint2.x; num6++)
					{
						gridPos.x = @uint.x + num6;
						uint num7 = PBDMath.MultilevelGridHash(gridPos);
						if (!CellStart.TryGetValue(num7, out var item))
						{
							continue;
						}
						for (; item < HashCount; item++)
						{
							if (num >= 16 && num2 >= 8)
							{
								break;
							}
							KeyValuePairComparable<uint, uint> keyValuePairComparable = Hash[(int)item];
							if (keyValuePairComparable.Key != num7)
							{
								break;
							}
							if (num < 16 && keyValuePairComparable.Value >= CollidersRange.x && keyValuePairComparable.Value < CollidersRange.y)
							{
								float3 min = AabbMin[(int)keyValuePairComparable.Value];
								float3 max = AabbMax[(int)keyValuePairComparable.Value];
								if (PBDMath.TestAABBOverlap(float2, float3, min, max))
								{
									BodyColliderPairs[index * 16 + num] = (int)(keyValuePairComparable.Value - CollidersRange.x);
									num++;
								}
							}
							if (num2 < 8 && keyValuePairComparable.Value >= ForceVolumesRange.x && keyValuePairComparable.Value < ForceVolumesRange.y)
							{
								float3 min2 = AabbMin[(int)keyValuePairComparable.Value];
								float3 max2 = AabbMax[(int)keyValuePairComparable.Value];
								if (PBDMath.TestAABBOverlap(float2, float3, min2, max2))
								{
									BodyForceVolumePairs[index * 8 + num2] = (int)(keyValuePairComparable.Value - ForceVolumesRange.x);
									num2++;
								}
							}
						}
					}
				}
			}
			for (int i = num; i < 16; i++)
			{
				BodyColliderPairs[index * 16 + i] = -1;
			}
			for (int j = num2; j < 8; j++)
			{
				BodyForceVolumePairs[index * 8 + j] = -1;
			}
		}
	}
}
