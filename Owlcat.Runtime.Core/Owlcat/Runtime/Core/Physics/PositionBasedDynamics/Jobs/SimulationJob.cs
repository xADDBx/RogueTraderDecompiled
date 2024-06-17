using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Math.Noise;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;

[BurstCompile]
public struct SimulationJob : IJobParallelFor
{
	[ReadOnly]
	public int SimulationIterations;

	[ReadOnly]
	public int ConstraintIterations;

	[ReadOnly]
	public float Decay;

	[ReadOnly]
	public float DeltaTime;

	[ReadOnly]
	public float VelocitySleepThreshold;

	[ReadOnly]
	public bool UseExperimentalFeatures;

	[ReadOnly]
	public float3 Gravity;

	[ReadOnly]
	public float StrengthNoiseWeight;

	[ReadOnly]
	public float StrengthNoiseContrast;

	[ReadOnly]
	public float2 WindVector;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> CompressedStrengthOctaves;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> CompressedShiftOctaves;

	[ReadOnly]
	public NativeArray<int> BodyDescriptorsIndices;

	[ReadOnly]
	public NativeArray<int2> ParticlesOffsetCount;

	[ReadOnly]
	public NativeArray<int2> ConstraintsOffsetCount;

	[ReadOnly]
	public NativeArray<int2> LocalCollidersOffsetCount;

	[ReadOnly]
	public NativeArray<float2> MaterialParameters;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> BasePosition;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Position;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Predicted;

	[NativeDisableParallelForRestriction]
	public NativeArray<float3> Velocity;

	[NativeDisableParallelForRestriction]
	public NativeArray<quaternion> Orientation;

	[NativeDisableParallelForRestriction]
	public NativeArray<quaternion> PredictedOrientation;

	[NativeDisableParallelForRestriction]
	public NativeArray<float4> AngularVelocity;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> Mass;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float> Radius;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<uint> Flags;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Index0;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Index1;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Index2;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Index3;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> Parameters0;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> Parameters1;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> Type;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ColliderParameters0;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ColliderParameters1;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4> ColliderParameters2;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float2> ColliderMaterialParameters;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> ColliderTypeList;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> ForceVolumeEnumPackedValues;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x2> ForceVolumeParameters;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float4x3> ForceVolumeEmissionParameters;

	[ReadOnly]
	public BroadphaseType BroadphaseType;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<float3> BroadphaseSceneAabb;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BodyColliderPairs;

	[ReadOnly]
	[NativeDisableParallelForRestriction]
	public NativeArray<int> BodyForceVolumePairs;

	[ReadOnly]
	public int BroadphaseGridResolution;

	public void Execute(int index)
	{
		int index2 = BodyDescriptorsIndices[index];
		int2 @int = ParticlesOffsetCount[index2];
		int2 int2 = ConstraintsOffsetCount[index2];
		int2 int3 = LocalCollidersOffsetCount[index2];
		float2 @float = MaterialParameters[index2];
		float bodyFriction = @float.x;
		float bodyRestitution = @float.y;
		float dt = DeltaTime * math.rcp(SimulationIterations);
		float di = math.rcp(SimulationIterations * ConstraintIterations);
		float num = dt * VelocitySleepThreshold;
		num *= num;
		float3 gridAabbMin = BroadphaseSceneAabb[0];
		float3 gridAabbMax = BroadphaseSceneAabb[1];
		float3 float2 = gridAabbMax - gridAabbMin;
		float2 gridCellSize = new float2(float2.x / (float)BroadphaseGridResolution, float2.z / (float)BroadphaseGridResolution);
		float3 gridAabbMin2 = BroadphaseSceneAabb[2];
		float3 gridAabbMax2 = BroadphaseSceneAabb[3];
		float3 float3 = gridAabbMax2 - gridAabbMin2;
		float2 gridCellSize2 = new float2(float3.x / (float)BroadphaseGridResolution, float3.z / (float)BroadphaseGridResolution);
		NativeArray<CollisionContact> contacts = new NativeArray<CollisionContact>(@int.y, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
		for (int i = 0; i < SimulationIterations; i++)
		{
			ApplyForces(ref index, ref @int.x, ref @int.y, ref dt, ref gridAabbMin2, ref gridAabbMax2, ref gridCellSize2);
			EstimatePositions(ref @int.x, ref @int.y, ref dt);
			FindContacts(ref index, ref @int.x, ref @int.y, ref int3.x, ref int3.y, ref bodyRestitution, ref bodyFriction, ref contacts, ref gridAabbMin, ref gridAabbMax, ref gridCellSize);
			for (int j = 0; j < ConstraintIterations; j++)
			{
				ResolveConstraints(ref int2.x, ref int2.y, ref di);
				ResolveContacts(ref @int.x, ref @int.y, ref contacts);
			}
			UpdateVelocities(ref @int.x, ref @int.y, ref dt, ref num);
			ConstrainVelocities(ref @int.x, ref @int.y, ref contacts);
			UpdatePositions(ref @int.x, ref @int.y);
		}
		contacts.Dispose();
	}

	private void ApplyForces(ref int bodyIndex, ref int particlesOffset, ref int particlesCount, ref float dt, ref float3 gridAabbMin, ref float3 gridAabbMax, ref float2 gridCellSize)
	{
		for (int i = particlesOffset; i < particlesOffset + particlesCount; i++)
		{
			float num = Mass[i];
			if (num <= 0f)
			{
				continue;
			}
			float3 value = Velocity[i];
			uint flags = Flags[i];
			float3 position = Position[i];
			float num2 = ((num > 0f) ? math.rcp(num) : 0f);
			if (!flags.HasFlag(ParticleFlags.SkipGlobalGravity))
			{
				value += dt * Gravity * num;
			}
			if (!flags.HasFlag(ParticleFlags.SkipGlobalWind))
			{
				float num3 = 0f;
				for (int j = 0; j < CompressedStrengthOctaves.Length; j++)
				{
					float4 @float = CompressedStrengthOctaves[j];
					num3 += (SimplexNoise2D.snoise(position.xz * @float.y - @float.zw) + 1f) * 0.5f * @float.x;
				}
				float y = math.saturate(0.5f + StrengthNoiseContrast * (num3 - 0.5f));
				y = math.lerp(1f, y, StrengthNoiseWeight);
				float num4 = 0f;
				for (int k = 0; k < CompressedShiftOctaves.Length; k++)
				{
					float4 float2 = CompressedShiftOctaves[k];
					num4 += SimplexNoise2D.snoise(position.xz * float2.y - float2.zw) * float2.x;
				}
				float2 float3 = WindVector;
				if (num4 > 0f)
				{
					float3 = math.mul(float2x2.Rotate(num4), WindVector);
				}
				value.xz += float3 * y * dt * num2;
			}
			switch (BroadphaseType)
			{
			case BroadphaseType.SimpleGrid:
				if (position.x >= gridAabbMin.x && position.z >= gridAabbMin.z && position.x <= gridAabbMax.x && position.z <= gridAabbMax.z)
				{
					float2 float4 = new float2(position.x - gridAabbMin.x, position.z - gridAabbMin.z);
					int2 x = new int2((int)(float4.x / gridCellSize.x), (int)(float4.y / gridCellSize.y));
					x = math.clamp(x, 0, BroadphaseGridResolution - 1);
					int num7 = x.y * BroadphaseGridResolution * 17 + x.x * 17;
					int num8 = BodyForceVolumePairs[num7];
					for (int m = 0; m < num8; m++)
					{
						int forceVolumeIndex = BodyForceVolumePairs[num7 + m + 1];
						value += dt * num2 * GetForce(forceVolumeIndex, position);
					}
				}
				break;
			case BroadphaseType.MultilevelGrid:
			{
				int num9 = bodyIndex * 8;
				for (int n = 0; n < 8; n++)
				{
					int num10 = BodyForceVolumePairs[num9 + n];
					if (num10 < 0)
					{
						break;
					}
					value += dt * num2 * GetForce(num10, position);
				}
				break;
			}
			case BroadphaseType.OptimizedSpatialHashing:
			{
				int num5 = bodyIndex * 8;
				for (int l = 0; l < 8; l++)
				{
					int num6 = BodyForceVolumePairs[num5 + l];
					if (num6 < 0)
					{
						break;
					}
					value += dt * num2 * GetForce(num6, position);
				}
				break;
			}
			}
			Velocity[i] = value;
		}
	}

	private float3 GetForce(int forceVolumeIndex, float3 position)
	{
		ForceVolume.UnpackEnumValues(ForceVolumeEnumPackedValues[forceVolumeIndex], out var volumeType, out var emissionType, out var _, out var _);
		bool flag = false;
		float3 result = default(float3);
		float4x2 float4x = ForceVolumeParameters[forceVolumeIndex];
		switch (volumeType)
		{
		case ForceVolumeType.Sphere:
		{
			float4 c3 = float4x.c0;
			float w2 = c3.w;
			flag = math.distancesq(c3.xyz, position) < w2 * w2;
			break;
		}
		case ForceVolumeType.Cylinder:
		{
			float4 c4 = float4x.c0;
			float4 c5 = float4x.c1;
			float3 lineDir = math.normalize(c5.xyz - c4.xyz);
			float4 @float = new float4(lineDir.xyz, 0f - math.dot(lineDir.xyz, c4.xyz));
			float4 float2 = new float4(-lineDir.xyz, 0f - math.dot(-lineDir.xyz, c5.xyz));
			float num = math.dot(@float.xyz, position) + @float.w;
			float num2 = math.dot(float2.xyz, position) + float2.w;
			if (num > 0f && num2 > 0f)
			{
				flag = math.distance(PBDMath.NearestPointOnLine(c4.xyz, lineDir, position), position) < c4.w;
			}
			break;
		}
		case ForceVolumeType.Cone:
		{
			float4 c = float4x.c0;
			float w = c.w;
			if (math.distancesq(c.xyz, position) < w * w)
			{
				float4 c2 = float4x.c1;
				flag = math.dot(math.normalize(position - c.xyz), c2.xyz) > c2.w;
			}
			break;
		}
		}
		if (flag)
		{
			float4x3 float4x2 = ForceVolumeEmissionParameters[forceVolumeIndex];
			float4 c6 = float4x2.c0;
			float4 c7 = float4x2.c1;
			float4 c8 = float4x2.c2;
			switch (emissionType)
			{
			case ForceEmissionType.Point:
				result = math.normalize(position - c6.xyz);
				result = math.lerp(result, c7.xyz, c7.w);
				break;
			case ForceEmissionType.Axis:
			{
				float3 float4 = PBDMath.NearestPointOnLine(c6.xyz, c8.xyz, position);
				result = math.normalize(position - float4);
				result = math.lerp(result, c7.xyz, c7.w);
				break;
			}
			case ForceEmissionType.Vortex:
			{
				float3 float3 = PBDMath.NearestPointOnLine(c6.xyz, c8.xyz, position);
				result = math.normalize(position - float3);
				result = math.cross(c8.xyz, result);
				result = math.lerp(result, c7.xyz, c7.w);
				break;
			}
			case ForceEmissionType.Directional:
				result = c7.xyz;
				break;
			}
			result *= c6.w;
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static quaternion AngularVelocityToSpinQuaternion(quaternion rotation, float4 angularVelocity)
	{
		quaternion a = new quaternion(angularVelocity.x, angularVelocity.y, angularVelocity.z, 0f);
		return new quaternion(0.5f * math.mul(a, rotation).value);
	}

	private void EstimatePositions(ref int particlesOffset, ref int particlesCount, ref float dt)
	{
		for (int i = particlesOffset; i < particlesOffset + particlesCount; i++)
		{
			Predicted[i] = Position[i] + dt * Velocity[i] * Decay;
			if (UseExperimentalFeatures)
			{
				quaternion q = Orientation[i];
				q.value += AngularVelocityToSpinQuaternion(Orientation[i], AngularVelocity[i]).value * dt * Decay;
				PredictedOrientation[i] = math.normalize(q);
			}
		}
	}

	private void FindContacts(ref int bodyIndex, ref int particlesOffset, ref int particlesCount, ref int localCollidersOffset, ref int localCollidersCount, ref float bodyRestitution, ref float bodyFriction, ref NativeArray<CollisionContact> contacts, ref float3 gridAabbMin, ref float3 gridAabbMax, ref float2 gridCellSize)
	{
		for (int i = 0; i < particlesCount; i++)
		{
			int num = i + particlesOffset;
			CollisionContact contact = default(CollisionContact);
			contact.ColliderId = -1;
			contact.ParticleId = -1;
			if (Mass[num] <= 0f)
			{
				contacts[i] = contact;
				continue;
			}
			if (Flags[num].HasFlag(ParticleFlags.SkipCollision))
			{
				contacts[i] = contact;
				continue;
			}
			float3 predicted = Predicted[num];
			float radius = Radius[num];
			float restitution = bodyRestitution;
			float friction = bodyFriction;
			if (localCollidersCount > 0)
			{
				bool flag = false;
				int num2 = 0;
				while (!flag && num2 < localCollidersCount)
				{
					int colliderIndex = num2 + localCollidersOffset;
					flag = CheckContact(ref colliderIndex, ref predicted, ref radius, ref restitution, ref friction, ref contact);
					if (flag)
					{
						contact.ParticleId = num;
					}
					num2++;
				}
			}
			else
			{
				switch (BroadphaseType)
				{
				case BroadphaseType.SimpleGrid:
				{
					if (!(predicted.x + radius >= gridAabbMin.x) || !(predicted.z + radius >= gridAabbMin.z) || !(predicted.x - radius <= gridAabbMax.x) || !(predicted.z - radius <= gridAabbMax.z))
					{
						break;
					}
					float2 @float = new float2(predicted.x - gridAabbMin.x, predicted.z - gridAabbMin.z);
					int2 x = new int2((int)(@float.x / gridCellSize.x), (int)(@float.y / gridCellSize.y));
					x = math.clamp(x, 0, BroadphaseGridResolution - 1);
					int num4 = x.y * BroadphaseGridResolution * 17 + x.x * 17;
					int num5 = BodyColliderPairs[num4];
					bool flag2 = false;
					int num6 = 0;
					while (!flag2 && num6 < num5)
					{
						int colliderIndex3 = BodyColliderPairs[num4 + num6 + 1];
						flag2 = CheckContact(ref colliderIndex3, ref predicted, ref radius, ref restitution, ref friction, ref contact);
						if (flag2)
						{
							contact.ParticleId = num;
						}
						num6++;
					}
					break;
				}
				case BroadphaseType.MultilevelGrid:
				{
					int num7 = bodyIndex * 16;
					for (int k = 0; k < 16; k++)
					{
						int colliderIndex4 = BodyColliderPairs[num7 + k];
						if (colliderIndex4 < 0)
						{
							break;
						}
						if (CheckContact(ref colliderIndex4, ref predicted, ref radius, ref restitution, ref friction, ref contact))
						{
							contact.ParticleId = num;
							break;
						}
					}
					break;
				}
				case BroadphaseType.OptimizedSpatialHashing:
				{
					int num3 = bodyIndex * 16;
					for (int j = 0; j < 16; j++)
					{
						int colliderIndex2 = BodyColliderPairs[num3 + j];
						if (colliderIndex2 < 0)
						{
							break;
						}
						if (CheckContact(ref colliderIndex2, ref predicted, ref radius, ref restitution, ref friction, ref contact))
						{
							contact.ParticleId = num;
							break;
						}
					}
					break;
				}
				}
			}
			contacts[i] = contact;
		}
	}

	private bool CheckContact(ref int colliderIndex, ref float3 predicted, ref float radius, ref float restitution, ref float friction, ref CollisionContact contact)
	{
		bool result = false;
		float4 x = ColliderParameters0[colliderIndex];
		float4 y = ColliderParameters1[colliderIndex];
		float2 @float = ColliderMaterialParameters[colliderIndex];
		float x2 = @float.x;
		float y2 = @float.y;
		switch ((ColliderType)ColliderTypeList[colliderIndex])
		{
		case ColliderType.Plane:
		{
			float num4 = math.dot(x.xyz, predicted) + x.w - radius;
			if (num4 < 0f)
			{
				contact.Normal = math.normalize(x.xyz);
				contact.Position = predicted + x.xyz * (0f - num4);
				contact.Restitution = math.min(y2, restitution);
				contact.Friction = math.max(x2, friction);
				contact.ColliderId = colliderIndex;
				result = true;
			}
			break;
		}
		case ColliderType.Sphere:
			if (math.distance(x.xyz, predicted) < x.w + radius)
			{
				contact.Normal = math.normalize(predicted - x.xyz);
				contact.Position = x.xyz + contact.Normal * (x.w + radius);
				contact.Restitution = math.min(y2, restitution);
				contact.Friction = math.max(x2, friction);
				contact.ColliderId = colliderIndex;
				result = true;
			}
			break;
		case ColliderType.Capsule:
		{
			float s = ClosestPointSegmentRatio(predicted, x.xyz, y.xyz);
			float4 float14 = math.lerp(x, y, s);
			if (math.distance(float14.xyz, predicted) < float14.w + radius)
			{
				contact.Normal = math.normalize(predicted - float14.xyz);
				contact.Position = float14.xyz + contact.Normal * (float14.w + radius);
				contact.Restitution = math.min(y2, restitution);
				contact.Friction = math.max(x2, friction);
				contact.ColliderId = colliderIndex;
				result = true;
			}
			break;
		}
		case ColliderType.Box:
		{
			float4 float2 = ColliderParameters2[colliderIndex];
			float3 xyz = x.xyz;
			float3 xyz2 = y.xyz;
			float3 xyz3 = float2.xyz;
			float3 float3 = new float3(x.w, y.w, float2.w);
			float3 float4 = math.normalize(xyz);
			float3 float5 = math.normalize(xyz2);
			float3 float6 = math.normalize(xyz3);
			float4 float7 = PBDMath.CreatePlane(float3 - xyz, -float4);
			float4 float8 = PBDMath.CreatePlane(float3 + xyz, float4);
			float4 float9 = PBDMath.CreatePlane(float3 - xyz2, -float5);
			float4 float10 = PBDMath.CreatePlane(float3 + xyz2, float5);
			float4 float11 = PBDMath.CreatePlane(float3 - xyz3, -float6);
			float4 float12 = PBDMath.CreatePlane(float3 + xyz3, float6);
			float num = -10000f;
			float3 float13 = new float3(1f, 0f, 0f);
			int num2 = 0;
			float num3 = math.dot(float7.xyz, predicted) + float7.w - radius;
			if (num3 < 0f)
			{
				num = num3;
				float13 = float7.xyz;
				num2++;
			}
			num3 = math.dot(float8.xyz, predicted) + float8.w - radius;
			if (num3 < 0f)
			{
				if (num < num3)
				{
					num = num3;
					float13 = float8.xyz;
				}
				num2++;
			}
			num3 = math.dot(float9.xyz, predicted) + float9.w - radius;
			if (num3 < 0f)
			{
				if (num < num3)
				{
					num = num3;
					float13 = float9.xyz;
				}
				num2++;
			}
			num3 = math.dot(float10.xyz, predicted) + float10.w - radius;
			if (num3 < 0f)
			{
				if (num < num3)
				{
					num = num3;
					float13 = float10.xyz;
				}
				num2++;
			}
			num3 = math.dot(float11.xyz, predicted) + float11.w - radius;
			if (num3 < 0f)
			{
				if (num < num3)
				{
					num = num3;
					float13 = float11.xyz;
				}
				num2++;
			}
			num3 = math.dot(float12.xyz, predicted) + float12.w - radius;
			if (num3 < 0f)
			{
				if (num < num3)
				{
					num = num3;
					float13 = float12.xyz;
				}
				num2++;
			}
			if (num2 == 6)
			{
				contact.Normal = math.normalize(float13);
				contact.Position = predicted + float13 * (0f - num);
				contact.Restitution = math.min(y2, restitution);
				contact.Friction = math.max(x2, friction);
				contact.ColliderId = colliderIndex;
				result = true;
			}
			break;
		}
		}
		return result;
	}

	public static float ClosestPointSegmentRatio(float3 c, float3 a, float3 b)
	{
		float3 @float = b - a;
		return math.saturate(math.dot(c - a, @float) / math.dot(@float, @float));
	}

	private void ResolveConstraints(ref int constraintsOffset, ref int constraintsCount, ref float di)
	{
		for (int i = constraintsOffset; i < constraintsOffset + constraintsCount; i++)
		{
			ConstraintType constraintType = (ConstraintType)Type[i];
			float4 constraintParameters = Parameters0[i];
			float4 constraintParameters2 = Parameters1[i];
			int num = Index0[i];
			int num2 = Index1[i];
			float mass = Mass[num];
			float mass2 = Mass[num2];
			float3 basePosition = BasePosition[num];
			float3 basePosition2 = BasePosition[num2];
			float3 predicted = Predicted[num];
			float3 predicted2 = Predicted[num2];
			float3 position = Position[num2];
			quaternion predictedOrientation = (UseExperimentalFeatures ? PredictedOrientation[num] : quaternion.identity);
			quaternion predictedOrientation2 = (UseExperimentalFeatures ? PredictedOrientation[num2] : quaternion.identity);
			switch (constraintType)
			{
			case ConstraintType.Distance:
				ResolveDistanceConstraint(ref mass, ref mass2, ref basePosition, ref basePosition2, ref constraintParameters, ref di, ref predicted, ref predicted2);
				break;
			case ConstraintType.DistanceAngular:
			case ConstraintType.Grass:
				ResolveDistanceAngularConstraint(ref mass, ref mass2, ref basePosition, ref basePosition2, ref constraintParameters, ref constraintParameters2, ref di, ref predicted, ref predicted2, ref position);
				break;
			case ConstraintType.ShapeMatching:
				ResolveShapeMatchingConstraint(ref mass, ref basePosition, ref constraintParameters, ref di, ref predicted);
				break;
			case ConstraintType.StretchShear:
				if (UseExperimentalFeatures)
				{
					ResolveStretchShearConstraint(ref mass, ref mass2, ref basePosition, ref basePosition2, ref predicted, ref predicted2, ref predictedOrientation, ref constraintParameters, ref constraintParameters2);
				}
				break;
			case ConstraintType.BendTwist:
				if (UseExperimentalFeatures)
				{
					ResolveBendTwistConstraint(ref mass, ref mass2, ref predictedOrientation, ref predictedOrientation2, ref constraintParameters, ref constraintParameters2);
				}
				break;
			}
			Predicted[num] = predicted;
			Predicted[num2] = predicted2;
			Position[num2] = position;
			PredictedOrientation[UseExperimentalFeatures ? num : 0] = predictedOrientation;
			PredictedOrientation[UseExperimentalFeatures ? num2 : 0] = predictedOrientation2;
		}
	}

	private void ResolveDistanceConstraint(ref float mass0, ref float mass1, ref float3 basePosition0, ref float3 basePosition1, ref float4 constraintParameters0, ref float di, ref float3 predicted0, ref float3 predicted1)
	{
		float x = constraintParameters0.x;
		float y = constraintParameters0.y;
		bool num = constraintParameters0.z > 0f;
		float num2 = ((mass0 > 0f) ? math.rcp(mass0) : 0f);
		float num3 = ((mass1 > 0f) ? math.rcp(mass1) : 0f);
		float num4 = math.distance(basePosition0, basePosition1);
		float num5 = (num ? 1f : (num2 + num3));
		float3 x2 = predicted1 - predicted0;
		float num6 = math.length(x2);
		x2 = ((num6 <= 0f) ? new float3(0f, 1f, 0f) : math.normalize(x2));
		float3 @float = ((!(num6 < num4)) ? (y * x2 * (num6 - num4) / num5) : (x * x2 * (num6 - num4) / num5));
		if (num)
		{
			if (mass1 > 0f)
			{
				predicted1 -= @float;
			}
			return;
		}
		if (mass0 > 0f)
		{
			predicted0 += num2 * @float;
		}
		if (mass1 > 0f)
		{
			predicted1 -= num3 * @float;
		}
	}

	private void ResolveDistanceAngularConstraint(ref float mass0, ref float mass1, ref float3 basePosition0, ref float3 basePosition1, ref float4 constraintParameters0, ref float4 constraintParameters1, ref float di, ref float3 predicted0, ref float3 predicted1, ref float3 position1)
	{
		float x = constraintParameters0.x;
		float y = constraintParameters0.y;
		float t = constraintParameters0.z;
		float w = constraintParameters0.w;
		bool num = constraintParameters1.y > 0f;
		float num2 = ((mass0 > 0f) ? math.rcp(mass0) : 0f);
		float num3 = ((mass1 > 0f) ? math.rcp(mass1) : 0f);
		float num4 = math.distance(basePosition0, basePosition1);
		float num5 = (num ? 1f : (num2 + num3));
		float3 from = predicted1 - predicted0;
		if (mass1 > 0f)
		{
			float3 to = basePosition1 - basePosition0;
			from = math.mul(FromToRotation(ref from, ref to, ref t), from);
			float3 @float = predicted0 + from - predicted1;
			float3 float2 = predicted1;
			predicted1 += @float * mass1 * di;
			float3 float3 = (predicted1 - float2) * (1f - w);
			position1 += float3;
		}
		float num6 = math.length(from);
		from = ((num6 <= 0f) ? new float3(0f, 1f, 0f) : math.normalize(from));
		float3 float4 = ((!(num6 < num4)) ? (y * from * (num6 - num4) / num5) : (x * from * (num6 - num4) / num5));
		if (num)
		{
			if (mass1 > 0f)
			{
				predicted1 -= float4;
			}
			return;
		}
		if (mass0 > 0f)
		{
			predicted0 += num2 * float4;
		}
		if (mass1 > 0f)
		{
			predicted1 -= num3 * float4;
		}
	}

	private quaternion FromToRotation(ref float3 from, ref float3 to, ref float t)
	{
		float3 x = math.normalize(from);
		float3 y = math.normalize(to);
		float num = math.dot(x, y);
		float num2 = math.acos(num);
		float3 axis = math.cross(x, y);
		if (math.abs(1f + num) < 1E-06f)
		{
			num2 = MathF.PI;
			axis = ((!(x.x > x.y) || !(x.x > x.z)) ? math.cross(x, new float3(1f, 0f, 0f)) : math.cross(x, new float3(0f, 1f, 0f)));
		}
		else if (math.abs(1f - num) < 1E-06f)
		{
			return quaternion.identity;
		}
		return quaternion.AxisAngle(axis, num2 * t);
	}

	private void ResolveShapeMatchingConstraint(ref float mass0, ref float3 basePosition0, ref float4 constraintParameters0, ref float di, ref float3 predicted0)
	{
		if (!(mass0 <= 0f))
		{
			float3 x = predicted0 - basePosition0;
			float num = math.length(x);
			x = math.normalize(x);
			if (!(num <= 1E-06f))
			{
				float3 @float = constraintParameters0.x * x * num * di;
				predicted0 -= @float;
			}
		}
	}

	private void ResolveStretchShearConstraint(ref float mass0, ref float mass1, ref float3 basePosition0, ref float3 basePosition1, ref float3 predicted0, ref float3 predicted1, ref quaternion predictedOrientation0, ref float4 constraintParameters0, ref float4 constraintParameters1)
	{
		float num = math.distance(basePosition0, basePosition1);
		float num2 = ((mass0 > 0f) ? math.rcp(mass0) : 0f);
		float num3 = ((mass1 > 0f) ? math.rcp(mass1) : 0f);
		quaternion quaternion = new quaternion(constraintParameters1);
		float3 @float = math.rotate(quaternion, new float3(0f, 0f, 1f));
		float3 v = (predicted1 - predicted0) / num - @float;
		v /= (num3 + num2) / num + num2 * 4f * num + 1E-06f;
		quaternion q = math.mul(predictedOrientation0, quaternion);
		v = math.rotate(math.conjugate(q), v);
		float3 v2 = v * 0.1f;
		v *= constraintParameters0.xyz;
		v = math.mul(q, v);
		v2 = math.mul(q, v2);
		predicted0 += num2 * v;
		predicted1 += (0f - num3) * v;
		quaternion b2 = math.mul(b: math.conjugate(new quaternion(@float.x, @float.y, @float.z, 0f)), a: predictedOrientation0);
		quaternion quaternion2 = math.mul(new quaternion(v2[0], v2[1], v2[2], 0f), b2);
		quaternion2.value *= 2f * num2 * num;
		predictedOrientation0.value += quaternion2.value;
		predictedOrientation0 = math.normalize(predictedOrientation0);
	}

	private void ResolveBendTwistConstraint(ref float mass0, ref float mass1, ref quaternion predictedOrientation0, ref quaternion predictedOrientation1, ref float4 constraintParameters0, ref float4 constraintParameters1)
	{
		float num = ((mass0 > 0f) ? math.rcp(mass0) : 0f);
		float num2 = ((mass1 > 0f) ? math.rcp(mass1) : 0f);
		quaternion b = math.mul(math.conjugate(predictedOrientation0), predictedOrientation1);
		quaternion quaternion = new quaternion(constraintParameters1);
		quaternion quaternion2 = default(quaternion);
		quaternion2.value = b.value + quaternion.value;
		b.value -= quaternion.value;
		if (math.lengthsq(b.value) > math.lengthsq(quaternion2.value))
		{
			b = quaternion2;
		}
		b.value.xyz *= constraintParameters0.xyz / new float3(num + num2 + 1E-06f);
		b.value.w = 0f;
		quaternion quaternion3 = math.mul(predictedOrientation1, b);
		quaternion quaternion4 = math.mul(predictedOrientation0, b);
		predictedOrientation0.value += quaternion3.value * num;
		predictedOrientation1.value += quaternion4.value * (0f - num2);
		predictedOrientation0 = math.normalize(predictedOrientation0);
		predictedOrientation1 = math.normalize(predictedOrientation1);
	}

	private void ResolveContacts(ref int particlesOffset, ref int particlesCount, ref NativeArray<CollisionContact> contacts)
	{
		for (int i = 0; i < particlesCount; i++)
		{
			CollisionContact collisionContact = contacts[i];
			if (collisionContact.ParticleId >= 0)
			{
				int index = i + particlesOffset;
				float3 @float = Predicted[index];
				float4 float2 = new float4(collisionContact.Normal, 0f - math.dot(collisionContact.Position, collisionContact.Normal));
				float num = math.dot(@float, float2.xyz) + float2.w;
				if (num < 0f)
				{
					@float -= float2.xyz * num;
				}
				Predicted[index] = @float;
			}
		}
	}

	private void UpdateVelocities(ref int particlesOffset, ref int particlesCount, ref float dt, ref float sleepTreshold)
	{
		for (int i = particlesOffset; i < particlesOffset + particlesCount; i++)
		{
			float3 @float = Predicted[i] - Position[i];
			float num = math.rcp(dt);
			float3 float2 = @float * num;
			if (math.dot(float2, float2) < sleepTreshold)
			{
				float2 = 0;
			}
			Velocity[i] = float2;
			if (UseExperimentalFeatures)
			{
				float4 float3 = new float4((math.mul(PredictedOrientation[i], math.inverse(Orientation[i])).value * 2f * num).xyz, 0f);
				if (math.dot(float3, float3) < sleepTreshold)
				{
					float3 = 0;
				}
				AngularVelocity[i] = float3;
			}
		}
	}

	private void ConstrainVelocities(ref int particlesOffset, ref int particlesCount, ref NativeArray<CollisionContact> contacts)
	{
		for (int i = 0; i < particlesCount; i++)
		{
			CollisionContact collisionContact = contacts[i];
			if (collisionContact.ParticleId >= 0)
			{
				int index = i + particlesOffset;
				float3 @float = Velocity[index];
				float3 float2 = @float;
				float2 -= 2f * math.dot(float2, collisionContact.Normal) * collisionContact.Normal;
				float3 float3 = -(float2 - math.dot(float2, collisionContact.Normal) * collisionContact.Normal);
				@float += float3 * collisionContact.Friction;
				@float *= collisionContact.Restitution;
				Velocity[index] = @float;
			}
		}
	}

	private void UpdatePositions(ref int particlesOffset, ref int particlesCount)
	{
		for (int i = particlesOffset; i < particlesOffset + particlesCount; i++)
		{
			Position[i] = Predicted[i];
			if (UseExperimentalFeatures)
			{
				Orientation[i] = PredictedOrientation[i];
			}
		}
	}
}
