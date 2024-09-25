using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct UpdateCollidersJob : IJobParallelFor
{
	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> ColliderTypeList;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ColliderParameters0;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ColliderParameters1;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ColliderParameters2;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float2> MaterialParameters;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ColliderAabbMin;

	[WriteOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> ColliderAabbMax;

	[ReadOnly]
	public NativeArray<int> Type;

	[ReadOnly]
	public NativeArray<float4x4> Matrices;

	[ReadOnly]
	public NativeArray<float4> Parameters0;

	[ReadOnly]
	public NativeArray<float4> MaterialParametersIn;

	[ReadOnly]
	public int BatchOffset;

	public void Execute(int index)
	{
		float4x4 localToWorld = Matrices[index];
		float4 @float = MaterialParametersIn[index];
		float x = @float.x;
		float y = @float.y;
		int index2 = index + BatchOffset;
		ColliderTypeList[index2] = Type[index];
		MaterialParameters[index2] = new float2(y, x);
		ColliderType colliderType = (ColliderType)Type[index];
		float4 collliderParameters = default(float4);
		float4 colliderParameters = default(float4);
		float4 colliderParameters2 = default(float4);
		float3 aabbMin = default(float3);
		float3 aabbMax = default(float3);
		switch (colliderType)
		{
		case ColliderType.Plane:
			UpdatePlane(ref collliderParameters, ref localToWorld);
			break;
		case ColliderType.Sphere:
		{
			float4 parameters3 = Parameters0[index];
			UpdateSphere(ref collliderParameters, ref localToWorld, ref parameters3, ref aabbMin, ref aabbMax);
			break;
		}
		case ColliderType.Capsule:
		{
			float4 parameters2 = Parameters0[index];
			UpdateCapsule(ref collliderParameters, ref colliderParameters, ref localToWorld, ref parameters2, ref aabbMin, ref aabbMax);
			break;
		}
		case ColliderType.Box:
		{
			float4 parameters = Parameters0[index];
			UpdateBox(ref collliderParameters, ref colliderParameters, ref colliderParameters2, ref localToWorld, ref parameters, ref aabbMin, ref aabbMax);
			break;
		}
		}
		ColliderParameters0[index2] = collliderParameters;
		ColliderParameters1[index2] = colliderParameters;
		ColliderParameters2[index2] = colliderParameters2;
		ColliderAabbMin[index2] = aabbMin;
		ColliderAabbMax[index2] = aabbMax;
	}

	private void UpdatePlane(ref float4 collliderParameters0, ref float4x4 localToWorld)
	{
		float4x4 v = math.inverse(localToWorld);
		v = math.transpose(v);
		collliderParameters0 = math.mul(v, new float4(0f, 1f, 0f, 0f));
		collliderParameters0.xyz = math.normalize(collliderParameters0.xyz);
	}

	private void UpdateSphere(ref float4 colliderParameters0, ref float4x4 localToWorld, ref float4 parameters0, ref float3 aabbMin, ref float3 aabbMax)
	{
		colliderParameters0 = math.mul(localToWorld, new float4(parameters0.xyz, 1f));
		float3 xyz = localToWorld.c0.xyz;
		float3 xyz2 = localToWorld.c1.xyz;
		float3 xyz3 = localToWorld.c2.xyz;
		float3 @float = new float3(math.length(xyz), math.length(xyz2), math.length(xyz3));
		float num = math.max(@float.x, math.max(@float.y, @float.z));
		colliderParameters0.w = parameters0.w * num;
		aabbMin = colliderParameters0.xyz - colliderParameters0.w;
		aabbMax = colliderParameters0.xyz + colliderParameters0.w;
	}

	private void UpdateCapsule(ref float4 colliderParameters0, ref float4 colliderParameters1, ref float4x4 localToWorld, ref float4 parameters0, ref float3 aabbMin, ref float3 aabbMax)
	{
		if (parameters0.x < 0f)
		{
			colliderParameters0 = localToWorld.c0;
			colliderParameters1 = localToWorld.c1;
		}
		else
		{
			float3 xyz = localToWorld.c0.xyz;
			float3 xyz2 = localToWorld.c1.xyz;
			float3 xyz3 = localToWorld.c2.xyz;
			float3 @float = new float3(math.length(xyz), math.length(xyz2), math.length(xyz3));
			float num = 1f;
			float num2 = 1f;
			float3 b = new float3(0f, 1f, 0f);
			switch ((CapsuleColliderDirection)(int)parameters0.x)
			{
			case CapsuleColliderDirection.AxisY:
				num = @float.y;
				num2 = math.max(@float.x, @float.z);
				b = new float3(0f, 1f, 0f);
				break;
			case CapsuleColliderDirection.AxisZ:
				num = @float.z;
				num2 = math.max(@float.x, @float.y);
				b = new float3(0f, 0f, 1f);
				break;
			case CapsuleColliderDirection.AxisX:
				num = @float.x;
				num2 = math.max(@float.y, @float.z);
				b = new float3(1f, 0f, 0f);
				break;
			}
			b = math.normalize(math.rotate(localToWorld, b));
			float3 xyz4 = localToWorld[3].xyz;
			float num3 = parameters0.y * num2;
			float num4 = parameters0.z * num2;
			float num5 = parameters0.w * 0.5f;
			float3 xyz5 = xyz4 - b * num * num5 + b * num3;
			float3 xyz6 = xyz4 + b * num * num5 - b * num4;
			colliderParameters0.xyz = xyz5;
			colliderParameters1.xyz = xyz6;
			colliderParameters0.w = num3;
			colliderParameters1.w = num4;
		}
		aabbMin = math.min(colliderParameters0.xyz - colliderParameters0.w, colliderParameters1.xyz - colliderParameters1.w);
		aabbMax = math.max(colliderParameters0.xyz + colliderParameters0.w, colliderParameters1.xyz + colliderParameters1.w);
	}

	private void UpdateBox(ref float4 colliderParameters0, ref float4 colliderParameters1, ref float4 colliderParameters2, ref float4x4 localToWorld, ref float4 parameters0, ref float3 aabbMin, ref float3 aabbMax)
	{
		float3 xyz = localToWorld.c0.xyz;
		float3 xyz2 = localToWorld.c1.xyz;
		float3 xyz3 = localToWorld.c2.xyz;
		float3 @float = new float3(math.length(xyz), math.length(xyz2), math.length(xyz3)) * parameters0.xyz * 0.5f;
		localToWorld.c0.xyz = math.normalize(xyz) * @float.x;
		localToWorld.c1.xyz = math.normalize(xyz2) * @float.y;
		localToWorld.c2.xyz = math.normalize(xyz3) * @float.z;
		colliderParameters0 = new float4(localToWorld.c0.xyz, localToWorld.c3.x);
		colliderParameters1 = new float4(localToWorld.c1.xyz, localToWorld.c3.y);
		colliderParameters2 = new float4(localToWorld.c2.xyz, localToWorld.c3.z);
		aabbMin = localToWorld.c3.xyz;
		aabbMax = localToWorld.c3.xyz;
		float4 float2 = math.mul(b: new float4(-1f, -1f, -1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(-1f, 1f, -1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(1f, 1f, -1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(1f, -1f, -1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(-1f, -1f, 1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(-1f, 1f, 1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(1f, 1f, 1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
		float2 = math.mul(b: new float4(1f, -1f, 1f, 1f), a: localToWorld);
		aabbMin = math.min(aabbMin, float2.xyz);
		aabbMax = math.max(aabbMax, float2.xyz);
	}
}
