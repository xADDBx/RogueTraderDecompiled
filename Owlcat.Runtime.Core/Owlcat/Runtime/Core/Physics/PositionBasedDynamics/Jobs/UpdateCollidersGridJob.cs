using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateCollidersGridJob : IJob
{
	public NativeArray<int> Grid;

	public NativeArray<float3> GridAabb;

	[ReadOnly]
	public NativeArray<float3> CollidersAabbMin;

	[ReadOnly]
	public NativeArray<float3> CollidersAabbMax;

	[ReadOnly]
	public int GridResolution;

	[ReadOnly]
	public int GlobalCollidersCount;

	public void Execute()
	{
		float3 @float = new float3(float.MaxValue, float.MaxValue, float.MaxValue);
		float3 float2 = new float3(float.MinValue, float.MinValue, float.MinValue);
		for (int i = 0; i < GlobalCollidersCount; i++)
		{
			@float = math.min(@float, CollidersAabbMin[i]);
			float2 = math.max(float2, CollidersAabbMax[i]);
		}
		GridAabb[0] = @float;
		GridAabb[1] = float2;
		float3 float3 = float2 - @float;
		float2 float4 = new float2(float3.x / (float)GridResolution, float3.z / (float)GridResolution);
		for (int j = 0; j < Grid.Length; j++)
		{
			Grid[j] = 0;
		}
		for (int k = 0; k < GlobalCollidersCount; k++)
		{
			float3 float5 = CollidersAabbMin[k];
			float3 float6 = CollidersAabbMax[k];
			float3 float7 = float5 - @float;
			float3 float8 = float6 - @float;
			int2 x = new int2((int)(float7.x / float4.x), (int)(float7.z / float4.y));
			int2 x2 = new int2((int)(float8.x / float4.x) + 1, (int)(float8.z / float4.y) + 1);
			x = math.clamp(x, 0, GridResolution);
			x2 = math.clamp(x2, 0, GridResolution);
			for (int l = x.y; l < x2.y; l++)
			{
				for (int m = x.x; m < x2.x; m++)
				{
					int num = l * 17 * GridResolution + m * 17;
					int num2 = Grid[num];
					if (num2 < 17)
					{
						Grid[num + num2 + 1] = k;
						num2++;
						Grid[num] = num2;
					}
				}
			}
		}
	}
}
