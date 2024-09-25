using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Shadows;

[BurstCompile]
internal struct TetrahedronCalculator
{
	private const int kFaceCount = 4;

	private const int kPlanesPerFace = 4;

	public NativeArray<float4x4> PointScaleMatrices;

	public NativeArray<float4x4> TetrahedronMatrices;

	public NativeArray<float4x4> PointLightTexMatrices;

	public NativeArray<float4> FaceVectors;

	public NativeArray<float3> FacePlaneNormals;

	public NativeArray<float4x4> TempPointProjMatrices;

	public TetrahedronCalculator(Allocator allocator)
	{
		PointScaleMatrices = new NativeArray<float4x4>(4, allocator, NativeArrayOptions.UninitializedMemory);
		PointScaleMatrices[0] = float4x4.Scale(-1f, 1f, 1f);
		PointScaleMatrices[1] = float4x4.Scale(1f, -1f, 1f);
		PointScaleMatrices[2] = float4x4.Scale(-1f, 1f, 1f);
		PointScaleMatrices[3] = float4x4.Scale(1f, -1f, 1f);
		TetrahedronMatrices = new NativeArray<float4x4>(4, allocator, NativeArrayOptions.UninitializedMemory);
		Matrix4x4 matrix4x = CreateRotationY(180f);
		Matrix4x4 matrix4x2 = CreateRotationX(27.367805f);
		TetrahedronMatrices[0] = matrix4x2 * matrix4x;
		matrix4x = CreateRotationY(0f);
		matrix4x2 = CreateRotationX(27.367805f);
		Matrix4x4 matrix4x3 = CreateRotationZ(90f);
		TetrahedronMatrices[1] = matrix4x3 * matrix4x2 * matrix4x;
		matrix4x = CreateRotationY(270f);
		matrix4x2 = CreateRotationX(-27.367805f);
		TetrahedronMatrices[2] = matrix4x2 * matrix4x;
		matrix4x = CreateRotationY(90f);
		matrix4x2 = CreateRotationX(-27.367805f);
		matrix4x3 = CreateRotationZ(90f);
		TetrahedronMatrices[3] = matrix4x3 * matrix4x2 * matrix4x;
		PointLightTexMatrices = new NativeArray<float4x4>(4, allocator, NativeArrayOptions.UninitializedMemory);
		float4x4 value = new float4x4
		{
			c0 = new float4(1f, 0f, 0f, 0f),
			c1 = new float4(0f, 0.5f, 0f, 0f),
			c2 = new float4(0f, 0f, 1f, 0f),
			c3 = new float4(0f, 0.5f, 0f, 1f)
		};
		PointLightTexMatrices[0] = value;
		value.c0 = new float4(0.5f, 0f, 0f, 0f);
		value.c1 = new float4(0f, 1f, 0f, 0f);
		value.c2 = new float4(0f, 0f, 1f, 0f);
		value.c3 = new float4(0.5f, 0f, 0f, 1f);
		PointLightTexMatrices[1] = value;
		value.c0 = new float4(1f, 0f, 0f, 0f);
		value.c1 = new float4(0f, 0.5f, 0f, 0f);
		value.c2 = new float4(0f, 0f, 1f, 0f);
		value.c3 = new float4(0f, -0.5f, 0f, 1f);
		PointLightTexMatrices[2] = value;
		value.c0 = new float4(0.5f, 0f, 0f, 0f);
		value.c1 = new float4(0f, 1f, 0f, 0f);
		value.c2 = new float4(0f, 0f, 1f, 0f);
		value.c3 = new float4(-0.5f, 0f, 0f, 1f);
		PointLightTexMatrices[3] = value;
		FaceVectors = new NativeArray<float4>(4, allocator, NativeArrayOptions.UninitializedMemory);
		FaceVectors[0] = new float4(0f, -0.57735026f, 0.8164966f, 0f);
		FaceVectors[1] = new float4(0f, -0.57735026f, -0.8164966f, 0f);
		FaceVectors[2] = new float4(-0.8164966f, 0.57735026f, 0f, 0f);
		FaceVectors[3] = new float4(0.8164966f, 0.57735026f, 0f, 0f);
		FacePlaneNormals = new NativeArray<float3>(16, allocator, NativeArrayOptions.UninitializedMemory);
		FacePlaneNormals[0] = CalculateNormal(-FaceVectors[3].xyz, -FaceVectors[2].xyz);
		FacePlaneNormals[1] = CalculateNormal(-FaceVectors[2].xyz, -FaceVectors[1].xyz);
		FacePlaneNormals[2] = CalculateNormal(-FaceVectors[1].xyz, -FaceVectors[3].xyz);
		FacePlaneNormals[3] = math.normalize(FaceVectors[0].xyz);
		FacePlaneNormals[4] = CalculateNormal(-FaceVectors[2].xyz, -FaceVectors[3].xyz);
		FacePlaneNormals[5] = CalculateNormal(-FaceVectors[3].xyz, -FaceVectors[0].xyz);
		FacePlaneNormals[6] = CalculateNormal(-FaceVectors[0].xyz, -FaceVectors[2].xyz);
		FacePlaneNormals[7] = math.normalize(FaceVectors[1].xyz);
		FacePlaneNormals[8] = CalculateNormal(-FaceVectors[3].xyz, -FaceVectors[1].xyz);
		FacePlaneNormals[9] = CalculateNormal(-FaceVectors[0].xyz, -FaceVectors[3].xyz);
		FacePlaneNormals[10] = CalculateNormal(-FaceVectors[1].xyz, -FaceVectors[0].xyz);
		FacePlaneNormals[11] = math.normalize(FaceVectors[2].xyz);
		FacePlaneNormals[12] = CalculateNormal(-FaceVectors[1].xyz, -FaceVectors[2].xyz);
		FacePlaneNormals[13] = CalculateNormal(-FaceVectors[0].xyz, -FaceVectors[1].xyz);
		FacePlaneNormals[14] = CalculateNormal(-FaceVectors[2].xyz, -FaceVectors[0].xyz);
		FacePlaneNormals[15] = math.normalize(FaceVectors[3].xyz);
		TempPointProjMatrices = new NativeArray<float4x4>(2, allocator, NativeArrayOptions.UninitializedMemory);
	}

	public void Dispose()
	{
		PointScaleMatrices.Dispose();
		TetrahedronMatrices.Dispose();
		PointLightTexMatrices.Dispose();
		FaceVectors.Dispose();
		FacePlaneNormals.Dispose();
		TempPointProjMatrices.Dispose();
	}

	private static float3 CalculateNormal(float3 v0, float3 v1)
	{
		return math.normalize(math.cross(v0, v1));
	}

	private static Matrix4x4 CreateRotationX(float angle)
	{
		return Matrix4x4.Rotate(Quaternion.Euler(angle, 0f, 0f));
	}

	private static Matrix4x4 CreateRotationY(float angle)
	{
		return Matrix4x4.Rotate(Quaternion.Euler(0f, angle, 0f));
	}

	private static Matrix4x4 CreateRotationZ(float angle)
	{
		return Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, angle));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void GetFacePlanes(in float3 lightPos, int faceId, ref ShadowSplitData shadowSplitData)
	{
		shadowSplitData.cullingPlaneCount = 4;
		for (int i = 0; i < 4; i++)
		{
			shadowSplitData.SetCullingPlane(i, new Plane(FacePlaneNormals[faceId * 4 + i], lightPos));
		}
	}
}
