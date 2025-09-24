using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
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
	public NativeSlice<ParticlePositionPair> PositionPairs;

	public void ProcessAnimation(AnimationStream stream)
	{
		if (PositionPairs.Length == 0 || PositionPairs.Length != BoneHandles.Length)
		{
			return;
		}
		RootSceneHandle.GetGlobalTR(stream, out var position, out var rotation);
		Matrix4x4 matrix4x = Matrix4x4.TRS(position, rotation, Vector3.one);
		Matrix4x4 inverse = matrix4x.inverse;
		int length = BoneHandles.Length;
		for (int i = 0; i < length; i++)
		{
			float3 position2 = PositionPairs[i].Position;
			TransformStreamHandle transformStreamHandle = BoneHandles[i];
			transformStreamHandle.SetPosition(stream, position2);
			int num = ParentMap[i];
			if (num > -1)
			{
				float3 basePosition = PositionPairs[i].BasePosition;
				Matrix4x4 matrix4x2 = matrix4x * Boneposes[i];
				float3 position3 = PositionPairs[num].Position;
				float3 basePosition2 = PositionPairs[num].BasePosition;
				float3 @float = basePosition - basePosition2;
				float3 float2 = position2 - position3;
				@float = inverse.MultiplyVector(@float);
				float2 = inverse.MultiplyVector(float2);
				float3 x = math.cross(@float, float2);
				x = math.normalize(x);
				Quaternion quaternion = Quaternion.AngleAxis(Vector3.Angle(@float, float2), x);
				transformStreamHandle.SetRotation(stream, matrix4x2.rotation * quaternion);
			}
		}
	}

	public void ProcessRootMotion(AnimationStream stream)
	{
	}
}
