using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Animations;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct PBDSkinnedBodyAnimationJob : IAnimationJob
{
	public TransformSceneHandle RootSceneHandle;

	public TransformStreamHandle RootStreamHandle;

	public NativeArray<TransformStreamHandle> BoneHandles;

	[ReadOnly]
	public NativeArray<Matrix4x4> Boneposes;

	[ReadOnly]
	public NativeArray<int> ParentMap;

	[ReadOnly]
	public NativeSlice<float3> BasePositions;

	[ReadOnly]
	public NativeSlice<float3> Positions;

	public void ProcessAnimation(AnimationStream stream)
	{
		if (Positions.Length == 0 || Positions.Length != BoneHandles.Length)
		{
			return;
		}
		RootSceneHandle.GetGlobalTR(stream, out var position, out var rotation);
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		Matrix4x4 inverse = matrix4x.inverse;
		int length = BoneHandles.Length;
		for (int i = 0; i < length; i++)
		{
			float3 @float = Positions[i];
			TransformStreamHandle transformStreamHandle = BoneHandles[i];
			transformStreamHandle.SetPosition(stream, @float);
			int num = ParentMap[i];
			if (num > -1)
			{
				float3 float2 = BasePositions[i];
				Matrix4x4 matrix4x2 = matrix4x * Boneposes[i];
				float3 float3 = Positions[num];
				float3 float4 = BasePositions[num];
				float3 float5 = float2 - float4;
				float3 float6 = @float - float3;
				float5 = inverse.MultiplyVector(float5);
				float6 = inverse.MultiplyVector(float6);
				float3 x = math.cross(float5, float6);
				x = math.normalize(x);
				Quaternion quaternion = Quaternion.AngleAxis(Vector3.Angle(float5, float6), x);
				transformStreamHandle.SetRotation(stream, matrix4x2.rotation * quaternion);
			}
		}
	}

	public void ProcessRootMotion(AnimationStream stream)
	{
	}
}
