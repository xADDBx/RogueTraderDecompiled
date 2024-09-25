using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct Line
{
	public float3 point;

	public float3 direction;

	public Line(float3 point, float3 direction)
	{
		this.point = point;
		this.direction = direction;
	}

	public static Line Points(float3 a, float3 b)
	{
		return new Line(a, math.normalize(b - a));
	}

	public static Line PointDirection(float3 p, float3 direction)
	{
		return new Line(p, direction);
	}
}
