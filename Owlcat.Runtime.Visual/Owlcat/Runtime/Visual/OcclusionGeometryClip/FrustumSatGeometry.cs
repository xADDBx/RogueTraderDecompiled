using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct FrustumSatGeometry : ISatGeometry
{
	public Frustum frustum;

	public ABox bounds;

	public float3 edge0;

	public float3 edge1;

	public float3 edge2;

	public float3 edge3;

	public float3 edge4;

	public float3 edge5;

	public float3 point0;

	public float3 point1;

	public float3 point2;

	public float3 point3;

	public float3 point4;

	public FrustumSatGeometry(float4x4 viewMatrix, float4x4 viewMatrixInverse, float3 targetRootPos, float2 targetSize)
	{
		float3 @float = math.transform(viewMatrix, targetRootPos);
		float3 float2 = new float3(targetSize.xy / 2f, 0f);
		float3 b = @float + new float3(-float2.xy, 0f);
		float3 b2 = @float + new float3(float2.x, 0f - float2.y, 0f);
		float3 b3 = @float + new float3(float2.xy, 0f);
		float3 b4 = @float + new float3(0f - float2.x, float2.y, 0f);
		float3 b5 = math.normalize(math.cross(new float3(0f, b3.yz), new float3(1f, 0f, 0f)));
		float3 b6 = math.normalize(math.cross(new float3(0f, b.yz), new float3(-1f, 0f, 0f)));
		float3 b7 = math.normalize(math.cross(new float3(b.x, 0f, b.z), new float3(0f, 1f, 0f)));
		float3 b8 = math.normalize(math.cross(new float3(b3.x, 0f, b.z), new float3(0f, -1f, 0f)));
		float3 b9 = new float3(0f, 0f, 1f);
		float3 float3 = math.transform(viewMatrixInverse, default(float3));
		b5 = math.transform(viewMatrixInverse, b5) - float3;
		b6 = math.transform(viewMatrixInverse, b6) - float3;
		b7 = math.transform(viewMatrixInverse, b7) - float3;
		b8 = math.transform(viewMatrixInverse, b8) - float3;
		b9 = math.transform(viewMatrixInverse, b9) - float3;
		frustum.left = Plane.FromPointNormal(float3, b7);
		frustum.right = Plane.FromPointNormal(float3, b8);
		frustum.top = Plane.FromPointNormal(float3, b5);
		frustum.bottom = Plane.FromPointNormal(float3, b6);
		frustum.near = Plane.FromPointNormal(float3, -b9);
		frustum.far = Plane.FromPointNormal(targetRootPos, b9);
		point0 = float3;
		point1 = math.transform(viewMatrixInverse, b);
		point2 = math.transform(viewMatrixInverse, b2);
		point3 = math.transform(viewMatrixInverse, b4);
		point4 = math.transform(viewMatrixInverse, b3);
		float3 x = point0;
		x = math.min(x, point1);
		x = math.min(x, point2);
		x = math.min(x, point3);
		x = math.min(x, point4);
		float3 x2 = float3;
		x2 = math.max(x2, point1);
		x2 = math.max(x2, point2);
		x2 = math.max(x2, point3);
		x2 = math.max(x2, point4);
		bounds = new ABox(in x, in x2);
		edge0 = math.normalize(point1 - float3);
		edge1 = math.normalize(point2 - float3);
		edge2 = math.normalize(point3 - float3);
		edge3 = math.normalize(point4 - float3);
		edge4 = math.transform(viewMatrixInverse, new float3(1f, 0f, 0f)) - float3;
		edge5 = math.transform(viewMatrixInverse, new float3(0f, 0f, 0f)) - float3;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static FrustumSatGeometry Lerp(FrustumSatGeometry a, FrustumSatGeometry b, float t)
	{
		FrustumSatGeometry result = default(FrustumSatGeometry);
		result.frustum = Frustum.Lerp(in a.frustum, in b.frustum, t);
		result.bounds = ABox.Lerp(in a.bounds, in b.bounds, t);
		result.edge0 = math.normalize(math.lerp(a.edge0, b.edge0, t));
		result.edge1 = math.normalize(math.lerp(a.edge0, b.edge1, t));
		result.edge2 = math.normalize(math.lerp(a.edge0, b.edge2, t));
		result.edge3 = math.normalize(math.lerp(a.edge0, b.edge3, t));
		result.edge4 = math.normalize(math.lerp(a.edge0, b.edge4, t));
		result.edge5 = math.normalize(math.lerp(a.edge0, b.edge5, t));
		result.point0 = math.lerp(a.point0, b.point0, t);
		result.point1 = math.lerp(a.point1, b.point1, t);
		result.point2 = math.lerp(a.point2, b.point2, t);
		result.point3 = math.lerp(a.point3, b.point3, t);
		result.point4 = math.lerp(a.point4, b.point4, t);
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetFaceNormals(ref NativeSlice<float3> container)
	{
		container[0] = frustum.left.normal;
		container[1] = frustum.right.normal;
		container[2] = frustum.top.normal;
		container[3] = frustum.bottom.normal;
		container[4] = frustum.near.normal;
		container[5] = frustum.far.normal;
		return 6;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public int GetEdgeDirections(ref NativeSlice<float3> container)
	{
		container[0] = edge0;
		container[1] = edge1;
		container[2] = edge2;
		container[3] = edge3;
		container[4] = edge4;
		container[5] = edge5;
		return 6;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ProjectToAxis(float3 axis, ref float axisMin, ref float axisMax)
	{
		axisMin = (axisMax = math.dot(axis, point0));
		float y = math.dot(axis, point1);
		axisMax = math.max(axisMax, y);
		axisMin = math.min(axisMin, y);
		y = math.dot(axis, point2);
		axisMax = math.max(axisMax, y);
		axisMin = math.min(axisMin, y);
		y = math.dot(axis, point3);
		axisMax = math.max(axisMax, y);
		axisMin = math.min(axisMin, y);
		y = math.dot(axis, point4);
		axisMax = math.max(axisMax, y);
		axisMin = math.min(axisMin, y);
	}
}
