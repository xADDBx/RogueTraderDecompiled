using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct Plane
{
	public float3 normal;

	public float distance;

	public Plane(float3 normal, float distance)
	{
		this.normal = normal;
		this.distance = distance;
	}

	public Plane(float3 normal, float3 point)
	{
		this.normal = normal;
		distance = math.dot(normal, point);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Plane Lerp(in Plane a, in Plane b, in float t)
	{
		return new Plane(math.normalize(math.lerp(a.normal, b.normal, t)), math.lerp(a.distance, b.distance, t));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Plane FromPointNormal(float3 point, float3 normal)
	{
		Plane result = default(Plane);
		result.normal = normal;
		result.distance = math.dot(normal, point);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float SignedDistance(in float3 point)
	{
		return math.dot(normal, point) - distance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public float3 Project(float3 point)
	{
		return point - normal * SignedDistance(in point);
	}
}
