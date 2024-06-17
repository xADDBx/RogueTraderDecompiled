using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct Ray
{
	public float3 origin;

	public float length;

	public float3 direction;

	public float3 inverseDirection;

	public Ray(float3 origin, float length, float3 direction)
	{
		this.origin = origin;
		this.length = length;
		this.direction = direction;
		inverseDirection = 1f / direction;
	}

	public static Ray BeginEnd(float3 begin, float3 end)
	{
		float3 @float = end - begin;
		float num = math.length(@float);
		float3 float2 = @float / num;
		return new Ray(begin, num, float2);
	}
}
