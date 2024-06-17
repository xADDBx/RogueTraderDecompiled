using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.OcclusionGeometryClip;

[BurstCompile]
internal static class GeometryMath
{
	public enum TargetPositionRelativeToBox
	{
		Inside,
		OnFrontSide,
		OnBackSide
	}

	private static readonly float3 FlatX = new float3(0f, 1f, 1f);

	private static readonly float3 FlatY = new float3(1f, 0f, 1f);

	private static readonly float3 FlatZ = new float3(1f, 1f, 0f);

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(in ABox b, in Ray r, ref float outDistance)
	{
		float3 x = (b.min - r.origin) * r.inverseDirection;
		float3 y = (b.max - r.origin) * r.inverseDirection;
		float3 x2 = math.min(x, y);
		float3 x3 = math.max(x, y);
		float num = math.cmax(x2);
		float num2 = math.cmin(x3);
		if (num > num2)
		{
			return false;
		}
		bool c = num < 0f;
		outDistance = math.select(num, num2, c);
		return outDistance > 0f;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(in Plane p, in ABox b)
	{
		float num = math.csum(b.Extent * math.abs(p.normal));
		return math.abs(math.dot(p.normal, b.Center) - p.distance) <= num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SignedDistance(in Plane plane, in ABox b)
	{
		float num = math.csum(b.Extent * math.abs(plane.normal));
		return math.dot(plane.normal, b.Center) - plane.distance - num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float SignedDistance(in Plane plane, in float3 point)
	{
		return math.dot(plane.normal, point) - plane.distance;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersects(in Frustum f, in ABox b)
	{
		if (SignedDistance(in f.left, in b) < 0f && SignedDistance(in f.right, in b) < 0f && SignedDistance(in f.top, in b) < 0f && SignedDistance(in f.bottom, in b) < 0f && SignedDistance(in f.near, in b) < 0f)
		{
			return SignedDistance(in f.far, in b) < 0f;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersect(in Plane p1, in Plane p2, ref Line line)
	{
		float3 @float = math.cross(p1.normal, p2.normal);
		float num = math.lengthsq(@float);
		if (math.abs(num) < float.Epsilon)
		{
			return false;
		}
		float3 p3 = (math.cross(@float, p2.normal) * (0f - p1.distance) + math.cross(p1.normal, @float) * (0f - p2.distance)) / num;
		float3 direction = math.normalize(@float);
		line = Line.PointDirection(p3, direction);
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Line IntersectUnsafe(in Plane p1, in Plane p2)
	{
		float3 @float = math.cross(p1.normal, p2.normal);
		float num = math.lengthsq(@float);
		float3 p3 = (math.cross(@float, p2.normal) * (0f - p1.distance) + math.cross(p1.normal, @float) * (0f - p2.distance)) / num;
		float3 direction = math.normalize(@float);
		return Line.PointDirection(p3, direction);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Intersect(in Plane plane, in Line line, ref float3 point)
	{
		if (math.abs(math.dot(plane.normal, line.direction)) < float.Epsilon)
		{
			return false;
		}
		float num = (math.dot(plane.normal, plane.normal * plane.distance) - math.dot(plane.normal, line.point)) / math.dot(plane.normal, line.direction);
		point = line.point + line.direction * num;
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float3 IntersectUnsafe(in Plane plane, in Line line)
	{
		math.dot(plane.normal, line.direction);
		float num = (math.dot(plane.normal, plane.normal * plane.distance) - math.dot(plane.normal, line.point)) / math.dot(plane.normal, line.direction);
		return line.point + line.direction * num;
	}

	public static TargetPositionRelativeToBox GetTargetPositionRelativeToBox(in OBox orientedBounds, float3 viewPosWorldSpace, float3 targetPosWorldSpace)
	{
		float3 @float = orientedBounds.TransformInverse(viewPosWorldSpace);
		float3 point = orientedBounds.TransformInverse(targetPosWorldSpace);
		float3 extents = orientedBounds.extents;
		float3 float2 = math.select(new float3(-1), new float3(1), @float >= 0f);
		float3 float3 = extents * float2;
		float3 y = math.normalize(@float - float3);
		if (math.all(math.abs(point) <= extents))
		{
			return TargetPositionRelativeToBox.Inside;
		}
		float3 float4 = new float3(float2.x, 0f, 0f);
		if (math.dot(float4, y) > 0f)
		{
			Plane plane = Plane.FromPointNormal(float4 * extents, float4);
			if (SignedDistance(in plane, in point) > 0f)
			{
				return TargetPositionRelativeToBox.OnFrontSide;
			}
		}
		float4 = new float3(0f, float2.y, 0f);
		if (math.dot(float4, y) > 0f)
		{
			Plane plane = Plane.FromPointNormal(float4 * extents, float4);
			if (SignedDistance(in plane, in point) > 0f)
			{
				return TargetPositionRelativeToBox.OnFrontSide;
			}
		}
		float4 = new float3(0f, 0f, float2.z);
		if (math.dot(float4, y) > 0f)
		{
			Plane plane = Plane.FromPointNormal(float4 * extents, float4);
			if (SignedDistance(in plane, in point) > 0f)
			{
				return TargetPositionRelativeToBox.OnFrontSide;
			}
		}
		return TargetPositionRelativeToBox.OnBackSide;
	}

	public static void TestTargetAgainstBox(in OBox orientedBounds, float3 viewPosWorldSpace, float3 targetPosWorldSpace, bool backSideDistanceProjectMode, out TargetPositionRelativeToBox result, out float backSideDistance)
	{
		bool backSideDistanceProjectMode = backSideDistanceProjectMode;
		float3 cameraLocalPos = orientedBounds.TransformInverse(viewPosWorldSpace);
		float3 targetLocalPos = orientedBounds.TransformInverse(targetPosWorldSpace);
		float3 extents = orientedBounds.extents;
		float3 @float = math.select(new float3(-1), new float3(1), cameraLocalPos >= 0f);
		float3 float2 = extents * @float;
		float3 facePoint2 = extents * new float3(0f - @float.x, @float.y, @float.z);
		float3 facePoint3 = extents * new float3(@float.x, 0f - @float.y, @float.z);
		float3 facePoint4 = extents * new float3(@float.x, @float.y, 0f - @float.z);
		float3 faceNormX = new float3(@float.x, 0f, 0f);
		float3 faceNormY = new float3(0f, @float.y, 0f);
		float3 faceNormZ = new float3(0f, 0f, @float.z);
		float3 faceCenterX = faceNormX * extents;
		float3 faceCenterY = faceNormY * extents;
		float3 faceCenterZ = faceNormZ * extents;
		bool3 faceVisible = default(bool3);
		faceVisible.x = math.dot(faceNormX, math.normalize(cameraLocalPos - float2)) > 0f;
		faceVisible.y = math.dot(faceNormY, math.normalize(cameraLocalPos - float2)) > 0f;
		faceVisible.z = math.dot(faceNormZ, math.normalize(cameraLocalPos - float2)) > 0f;
		bool3 @bool = default(bool3);
		@bool.x = !IsTargetInFrontOfFace(in faceCenterX, in faceNormX);
		@bool.y = !IsTargetInFrontOfFace(in faceCenterY, in faceNormY);
		@bool.z = !IsTargetInFrontOfFace(in faceCenterZ, in faceNormZ);
		bool3 bool2 = default(bool3);
		float3 faceCenter2 = -faceCenterX;
		float3 faceNormal2 = -faceNormX;
		bool2.x = !IsTargetInFrontOfFace(in faceCenter2, in faceNormal2);
		faceCenter2 = -faceCenterY;
		faceNormal2 = -faceNormY;
		bool2.y = !IsTargetInFrontOfFace(in faceCenter2, in faceNormal2);
		faceCenter2 = -faceCenterZ;
		faceNormal2 = -faceNormZ;
		bool2.z = !IsTargetInFrontOfFace(in faceCenter2, in faceNormal2);
		if (IsTargetInsideBox())
		{
			result = TargetPositionRelativeToBox.Inside;
			backSideDistance = 0f;
			return;
		}
		if (IsTargetInFrontOfAnyVisibleFace())
		{
			result = TargetPositionRelativeToBox.OnFrontSide;
			backSideDistance = 0f;
			return;
		}
		float num = float.MaxValue;
		if (faceVisible.x && @bool.x)
		{
			if (!bool2.z)
			{
				float y = GetDistanceToEdgePlane(facePoint4, FlatY);
				num = math.min(num, y);
			}
			if (!bool2.y)
			{
				float y2 = GetDistanceToEdgePlane(facePoint3, new float3(1f, 1f, 0f));
				num = math.min(num, y2);
			}
		}
		if (faceVisible.y && @bool.y)
		{
			if (!bool2.x)
			{
				float y3 = GetDistanceToEdgePlane(facePoint2, new float3(1f, 1f, 0f));
				num = math.min(num, y3);
			}
			if (!bool2.z)
			{
				float y4 = GetDistanceToEdgePlane(facePoint4, new float3(0f, 1f, 1f));
				num = math.min(num, y4);
			}
		}
		if (faceVisible.z && @bool.z)
		{
			if (!bool2.x)
			{
				float y5 = GetDistanceToEdgePlane(facePoint2, new float3(1f, 0f, 1f));
				num = math.min(num, y5);
			}
			if (!bool2.y)
			{
				float y6 = GetDistanceToEdgePlane(facePoint3, new float3(0f, 1f, 1f));
				num = math.min(num, y6);
			}
		}
		bool num2;
		if (faceVisible.x ^ faceVisible.z)
		{
			if (!faceVisible.x)
			{
				if (@bool.z)
				{
					num2 = !@bool.x;
					goto IL_04fa;
				}
			}
			else if (@bool.x)
			{
				num2 = !@bool.z;
				goto IL_04fa;
			}
		}
		goto IL_0525;
		IL_05f8:
		bool num3;
		if (num3 != 0)
		{
			float y7 = GetDistanceToEdgePlane(float2, new float3(0f, 1f, 1f));
			num = math.min(num, y7);
		}
		goto IL_0623;
		IL_0525:
		bool num4;
		if (faceVisible.x ^ faceVisible.y)
		{
			if (!faceVisible.x)
			{
				if (@bool.y)
				{
					num4 = !@bool.x;
					goto IL_0579;
				}
			}
			else if (@bool.x)
			{
				num4 = !@bool.y;
				goto IL_0579;
			}
		}
		goto IL_05a4;
		IL_05a4:
		if (faceVisible.y ^ faceVisible.z)
		{
			if (!faceVisible.y)
			{
				if (@bool.z)
				{
					num3 = !@bool.y;
					goto IL_05f8;
				}
			}
			else if (@bool.y)
			{
				num3 = !@bool.z;
				goto IL_05f8;
			}
		}
		goto IL_0623;
		IL_04fa:
		if (num2)
		{
			float y8 = GetDistanceToEdgePlane(float2, new float3(1f, 0f, 1f));
			num = math.min(num, y8);
		}
		goto IL_0525;
		IL_0623:
		result = TargetPositionRelativeToBox.OnBackSide;
		backSideDistance = num;
		return;
		IL_0579:
		if (num4)
		{
			float y9 = GetDistanceToEdgePlane(float2, new float3(1f, 1f, 0f));
			num = math.min(num, y9);
		}
		goto IL_05a4;
		float GetDistanceToEdgePlane(float3 facePoint, float3 flatter)
		{
			float3 normal = math.normalize((cameraLocalPos - targetLocalPos) * flatter);
			Plane plane = Plane.FromPointNormal(facePoint, normal);
			float3 float3;
			if (backSideDistanceProjectMode)
			{
				float3 = plane.Project(targetLocalPos);
			}
			else
			{
				Line line = Line.Points(cameraLocalPos, targetLocalPos);
				float3 = IntersectUnsafe(in plane, in line);
			}
			return math.length(targetLocalPos - float3);
		}
		bool IsTargetInFrontOfAnyVisibleFace()
		{
			if (faceVisible.x && IsTargetInFrontOfFace(in faceCenterX, in faceNormX))
			{
				return true;
			}
			if (faceVisible.y && IsTargetInFrontOfFace(in faceCenterY, in faceNormY))
			{
				return true;
			}
			if (faceVisible.z && IsTargetInFrontOfFace(in faceCenterZ, in faceNormZ))
			{
				return true;
			}
			return false;
		}
		bool IsTargetInFrontOfFace(in float3 faceCenter, in float3 faceNormal)
		{
			Plane plane2 = Plane.FromPointNormal(faceCenter, faceNormal);
			return SignedDistance(in plane2, in targetLocalPos) > 0f;
		}
		bool IsTargetInsideBox()
		{
			return math.all(math.abs(targetLocalPos) <= extents);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool SatIntersects<TPrimitiveA, TPrimitiveB>(TPrimitiveA a, TPrimitiveB b) where TPrimitiveA : ISatGeometry where TPrimitiveB : ISatGeometry
	{
		using NativeArray<float3> array = new NativeArray<float3>(32, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		NativeSlice<float3> container = new NativeSlice<float3>(array, 0, 16);
		int faceNormals = a.GetFaceNormals(ref container);
		float axisMin = 0f;
		float axisMax = 0f;
		float axisMin2 = 0f;
		float axisMax2 = 0f;
		for (int i = 0; i < faceNormals; i++)
		{
			a.ProjectToAxis(container[i], ref axisMin, ref axisMax);
			b.ProjectToAxis(container[i], ref axisMin2, ref axisMax2);
			if (axisMax < axisMin2 || axisMax2 < axisMin)
			{
				return false;
			}
		}
		faceNormals = b.GetFaceNormals(ref container);
		for (int j = 0; j < faceNormals; j++)
		{
			a.ProjectToAxis(container[j], ref axisMin, ref axisMax);
			b.ProjectToAxis(container[j], ref axisMin2, ref axisMax2);
			if (axisMax < axisMin2 || axisMax2 < axisMin)
			{
				return false;
			}
		}
		NativeSlice<float3> container2 = new NativeSlice<float3>(array, 16, 16);
		faceNormals = a.GetEdgeDirections(ref container);
		int edgeDirections = b.GetEdgeDirections(ref container2);
		for (int k = 0; k < faceNormals; k++)
		{
			for (int l = 0; l < edgeDirections; l++)
			{
				float3 axis = math.cross(container[k], container2[l]);
				a.ProjectToAxis(axis, ref axisMin, ref axisMax);
				b.ProjectToAxis(axis, ref axisMin2, ref axisMax2);
				if (axisMax < axisMin2 || axisMax2 < axisMin)
				{
					return false;
				}
			}
		}
		return true;
	}
}
