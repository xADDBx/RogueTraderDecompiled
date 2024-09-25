using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
public struct Frustum
{
	public Plane left;

	public Plane right;

	public Plane top;

	public Plane bottom;

	public Plane near;

	public Plane far;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Frustum Lerp(in Frustum a, in Frustum b, float t)
	{
		Frustum result = default(Frustum);
		result.left = Plane.Lerp(in a.left, in b.left, in t);
		result.right = Plane.Lerp(in a.right, in b.right, in t);
		result.top = Plane.Lerp(in a.top, in b.top, in t);
		result.bottom = Plane.Lerp(in a.bottom, in b.bottom, in t);
		result.near = Plane.Lerp(in a.near, in b.near, in t);
		result.far = Plane.Lerp(in a.far, in b.far, in t);
		return result;
	}

	public static Frustum Build(float4x4 viewMatrix, float near, float far, float2 fovDeg)
	{
		float2 @float = math.radians(fovDeg / 2f);
		float3 b = new float3(0f, 0f, -1f);
		float3 b2 = new float3(0f, 0f, 1f);
		float3 b3 = math.rotate(quaternion.RotateY(0f - @float.x), new float3(-1f, 0f, 0f));
		float3 b4 = new float3(0f - b3.x, b3.y, b3.z);
		float3 b5 = math.rotate(quaternion.RotateX(0f - @float.y), new float3(0f, 1f, 0f));
		float3 b6 = new float3(b5.x, 0f - b5.y, b5.z);
		float3 float2 = math.transform(viewMatrix, default(float3));
		b = math.transform(viewMatrix, b) - float2;
		b2 = math.transform(viewMatrix, b2) - float2;
		b3 = math.transform(viewMatrix, b3) - float2;
		b4 = math.transform(viewMatrix, b4) - float2;
		b5 = math.transform(viewMatrix, b5) - float2;
		b6 = math.transform(viewMatrix, b6) - float2;
		Frustum result = default(Frustum);
		result.near = Plane.FromPointNormal(float2 + b2 * near, b);
		result.far = Plane.FromPointNormal(float2 + b2 * far, b2);
		result.left = Plane.FromPointNormal(float2, b3);
		result.right = Plane.FromPointNormal(float2, b4);
		result.top = Plane.FromPointNormal(float2, b5);
		result.bottom = Plane.FromPointNormal(float2, b6);
		return result;
	}

	public static Frustum Build(float4x4 viewMatrix, float4x4 viewMatrixInverse, float3 targetRootPos, float2 targetSize, bool nearFarPlanesBillboardX = false, bool nearFarPlanesBillboardY = false)
	{
		float3 @float = math.transform(viewMatrix, targetRootPos) + new float3(targetSize.xy * -0.5f, 0f);
		float3 float2 = @float + new float3(targetSize, 0f);
		float3 b = math.normalize(math.cross(new float3(0f, float2.yz), new float3(1f, 0f, 0f)));
		float3 b2 = math.normalize(math.cross(new float3(0f, @float.yz), new float3(-1f, 0f, 0f)));
		float3 b3 = math.normalize(math.cross(new float3(@float.x, 0f, @float.z), new float3(0f, 1f, 0f)));
		float3 b4 = math.normalize(math.cross(new float3(float2.x, 0f, @float.z), new float3(0f, -1f, 0f)));
		float3 float3 = math.transform(viewMatrixInverse, default(float3));
		b = math.transform(viewMatrixInverse, b) - float3;
		b2 = math.transform(viewMatrixInverse, b2) - float3;
		b3 = math.transform(viewMatrixInverse, b3) - float3;
		b4 = math.transform(viewMatrixInverse, b4) - float3;
		float3 x = ((!nearFarPlanesBillboardY) ? (targetRootPos - float3) : (math.transform(viewMatrixInverse, new float3(0f, 0f, 1f)) - float3));
		if (nearFarPlanesBillboardX)
		{
			x.y = 0f;
		}
		x = math.normalize(x);
		Frustum result = default(Frustum);
		result.left = Plane.FromPointNormal(float3, b3);
		result.right = Plane.FromPointNormal(float3, b4);
		result.top = Plane.FromPointNormal(float3, b);
		result.bottom = Plane.FromPointNormal(float3, b2);
		result.near = Plane.FromPointNormal(float3, -x);
		result.far = Plane.FromPointNormal(targetRootPos, x);
		return result;
	}
}
