using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions.Broadphase;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Jobs;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics;

public class Simulation
{
	private bool m_Gpu;

	private int m_SimulationIterations;

	private int m_ConstraintIterations;

	private float m_Decay;

	private MemoryManager m_MemoryManager;

	private BroadphaseBase m_Broadphase;

	internal bool IsGPU => m_Gpu;

	public bool IsEmpty => m_MemoryManager?.IsEmpty ?? true;

	public bool IsSceneInitialization => m_MemoryManager?.IsSceneInitialization ?? false;

	public Simulation(bool gpu, int simulationIterations, int constraintIterations, float decay)
	{
		m_Gpu = gpu;
		m_SimulationIterations = simulationIterations;
		m_ConstraintIterations = constraintIterations;
		m_Decay = decay;
		m_MemoryManager = new MemoryManager(gpu);
		if (!m_Gpu)
		{
			ResetBroadphase();
		}
	}

	~Simulation()
	{
		Dispose();
	}

	public void Dispose()
	{
		m_MemoryManager.Dispose();
		if (m_Broadphase != null)
		{
			m_Broadphase.Dispose();
		}
	}

	internal int GetBodyDescriptorIndex(Body body)
	{
		return m_MemoryManager.GetBodyDescriptorIndex(body);
	}

	internal void RefreshBodyParameters(Body body)
	{
		m_MemoryManager.RefreshBodyParameters(body);
	}

	internal void GetParticles(Body body, out ParticleSoASlice particles)
	{
		m_MemoryManager.GetParticles(body, out particles);
	}

	internal int GetParticlesOffset(Body body)
	{
		return m_MemoryManager.GetParticlesOffset(body);
	}

	internal MeshBodyVerticesSoA GetMeshData()
	{
		return m_MemoryManager.MeshBodyVerticesSoA;
	}

	internal Constraint GetConstraint(int constraintId)
	{
		return m_MemoryManager.GetConstraint(constraintId);
	}

	internal void SetConstraint(ref Constraint constraint)
	{
		m_MemoryManager.SetConstraint(ref constraint);
	}

	internal IEnumerable<IForce> GetForces()
	{
		return m_MemoryManager.Forces;
	}

	internal IEnumerable<Body> GetBodies()
	{
		return m_MemoryManager.BodyDescriptorsMap.Keys;
	}

	internal void RegisterBody(Body body)
	{
		m_MemoryManager.RegisterBody(body);
	}

	internal void RegisterForce(IForce force)
	{
		m_MemoryManager.RegisterForce(force);
	}

	internal void RegisterCollider(ColliderRef colliderRef)
	{
		m_MemoryManager.RegisterCollider(colliderRef);
	}

	internal void RegisterForceVolume(ForceVolume forceVolume)
	{
		m_MemoryManager.RegisterForceVolume(forceVolume);
	}

	internal void UnregisterBody(Body body)
	{
		m_MemoryManager.UnregisterBody(body);
	}

	internal void UnregisterForce(IForce force)
	{
		m_MemoryManager.UnregisterForce(force);
	}

	internal void UnregisterCollider(ColliderRef colliderRef)
	{
		m_MemoryManager.UnregisterCollider(colliderRef);
	}

	internal void UnregisterLocalColliders(Body body)
	{
		m_MemoryManager.UnregisterLocalColliders(body);
	}

	internal void UnregisterForceVolume(ForceVolume forceVolume)
	{
		m_MemoryManager.UnregisterForceVolume(forceVolume);
	}

	internal GPUData GetGPUData()
	{
		return m_MemoryManager.GpuData;
	}

	internal void MemoryReset()
	{
		m_MemoryManager.Reset();
	}

	internal void UpdateDirtyBodyData(Body body)
	{
		m_MemoryManager.UpdateDirtyBodyData(body);
	}

	internal void BeginSceneInitialization()
	{
		m_MemoryManager.BeginSceneInitialization();
	}

	internal void EndSceneInitialization()
	{
		m_MemoryManager.EndSceneInitialization();
	}

	public void DrawGizmos(Body body)
	{
		if (m_Gpu || !m_MemoryManager.BodyDescriptorsMap.TryGetValue(body, out var value))
		{
			return;
		}
		BodyDescriptor bodyDescriptor = m_MemoryManager.BodyDescriptorsSoA[value];
		Vector3 size = Vector3.one * 0.1f;
		for (int i = 0; i < bodyDescriptor.ParticlesOffsetCount.y; i++)
		{
			Particle particle = m_MemoryManager.ParticlesSoA[bodyDescriptor.ParticlesOffsetCount.x + i];
			Gizmos.color = ((particle.Mass > 0f) ? Color.green : Color.red);
			Gizmos.DrawCube(particle.Position, size);
		}
		Gizmos.color = Color.yellow;
		for (int j = 0; j < body.Constraints.Count; j++)
		{
			Constraint constraint = body.Constraints[j];
			switch (constraint.type)
			{
			case ConstraintType.Distance:
			{
				Particle particle9 = m_MemoryManager.ParticlesSoA[constraint.index0 + bodyDescriptor.ParticlesOffsetCount.x];
				Particle particle10 = m_MemoryManager.ParticlesSoA[constraint.index1 + bodyDescriptor.ParticlesOffsetCount.x];
				Gizmos.DrawLine(particle9.Position, particle10.Position);
				break;
			}
			case ConstraintType.DistanceAngular:
			{
				Particle particle7 = m_MemoryManager.ParticlesSoA[constraint.index0 + bodyDescriptor.ParticlesOffsetCount.x];
				Particle particle8 = m_MemoryManager.ParticlesSoA[constraint.index1 + bodyDescriptor.ParticlesOffsetCount.x];
				Gizmos.DrawLine(particle7.Position, particle8.Position);
				break;
			}
			case ConstraintType.ShapeMatching:
			{
				Particle particle6 = m_MemoryManager.ParticlesSoA[constraint.index0 + bodyDescriptor.ParticlesOffsetCount.x];
				Gizmos.DrawLine(particle6.BasePosition, particle6.Position);
				break;
			}
			case ConstraintType.Grass:
			{
				Particle particle4 = m_MemoryManager.ParticlesSoA[constraint.index0 + bodyDescriptor.ParticlesOffsetCount.x];
				Particle particle5 = m_MemoryManager.ParticlesSoA[constraint.index1 + bodyDescriptor.ParticlesOffsetCount.x];
				Gizmos.DrawLine(particle4.Position, particle5.Position);
				break;
			}
			case ConstraintType.StretchShear:
			{
				Particle particle2 = m_MemoryManager.ParticlesSoA[constraint.index0 + bodyDescriptor.ParticlesOffsetCount.x];
				Particle particle3 = m_MemoryManager.ParticlesSoA[constraint.index1 + bodyDescriptor.ParticlesOffsetCount.x];
				Gizmos.DrawLine(particle2.Position, particle3.Position);
				break;
			}
			}
		}
	}

	internal void DrawBroadphase()
	{
		if (PBD.IsGpu)
		{
			return;
		}
		Color color = Gizmos.color;
		switch (m_Broadphase.Type)
		{
		case BroadphaseType.SimpleGrid:
		{
			int gridResolution = PBD.BroadphaseSettings.SimpleGridSettings.GridResolution;
			float3 float5 = m_Broadphase.SceneAabb[0];
			float3 float6 = m_Broadphase.SceneAabb[1];
			float3 float7 = float6 - float5;
			float3 float8 = (float5 + float6) * 0.5f;
			Gizmos.color = PBD.DebugSettings.BroadphaseStructureColor;
			Gizmos.DrawWireCube(float8, float7);
			float2 float9 = new float2(float7.x / (float)gridResolution, float7.z / (float)gridResolution);
			for (int l = 0; l < gridResolution; l++)
			{
				for (int m = 0; m < gridResolution; m++)
				{
					int index3 = l * 17 * gridResolution + m * 17;
					if (m_Broadphase.BodyCollidersPairsSoA[index3] > 0)
					{
						Gizmos.color = PBD.DebugSettings.BodyColliderPairColor;
						Gizmos.DrawWireCube(new Vector3((float)m * float9.x, 0f, (float)l * float9.y) + new Vector3(float5.x + float9.x * 0.5f, 0f, float5.z + float9.y * 0.5f), new Vector3(float9.x, 1f, float9.y));
					}
				}
			}
			float3 float10 = m_Broadphase.SceneAabb[2];
			float3 float11 = m_Broadphase.SceneAabb[3];
			float3 float12 = float11 - float10;
			float3 float13 = (float10 + float11) * 0.5f;
			Gizmos.color = PBD.DebugSettings.BroadphaseStructureColor;
			Gizmos.DrawWireCube(float13, float12);
			float9 = new float2(float12.x / (float)gridResolution, float12.z / (float)gridResolution);
			for (int n = 0; n < gridResolution; n++)
			{
				for (int num5 = 0; num5 < gridResolution; num5++)
				{
					int index4 = n * 17 * gridResolution + num5 * 17;
					if (m_Broadphase.BodyForceVolumePairsSoA[index4] > 0)
					{
						Gizmos.color = PBD.DebugSettings.BodyForceVolumePairColor;
						Gizmos.DrawWireCube(new Vector3((float)num5 * float9.x, 0f, (float)n * float9.y) + new Vector3(float10.x + float9.x * 0.5f, 0f, float10.z + float9.y * 0.5f), new Vector3(float9.x, 1f, float9.y));
					}
				}
			}
			break;
		}
		case BroadphaseType.MultilevelGrid:
		{
			Gizmos.color = PBD.DebugSettings.BroadphaseStructureColor;
			NativeArray<float3> sceneAabb = m_Broadphase.SceneAabb;
			float3 float14 = sceneAabb[0];
			float3 float15 = sceneAabb[1];
			Gizmos.DrawWireCube((float14 + float15) * 0.5f, float15 - float14);
			if (!(m_Broadphase is MultilevelGridBroadphase { CellStart: var cellStart } multilevelGridBroadphase))
			{
				break;
			}
			NativeKeyValueArrays<uint, uint> keyValueArrays = cellStart.GetKeyValueArrays(Allocator.Temp);
			for (int num6 = 0; num6 < keyValueArrays.Keys.Length; num6++)
			{
				uint num7 = keyValueArrays.Keys[num6];
				uint num8 = num7 >> 30;
				uint num9 = (num7 >> 20) & 0x3FFu;
				uint num10 = (num7 >> 10) & 0x3FFu;
				uint num11 = num7 & 0x3FFu;
				float3 float16 = multilevelGridBroadphase.CellSize * (float)(1 << (int)num8);
				Gizmos.DrawWireCube(new float3(num9, num10, num11) * float16 + float14 + float16 * 0.5f, float16);
			}
			keyValueArrays.Dispose();
			for (int num12 = 0; num12 < m_MemoryManager.BodiesCount; num12++)
			{
				float3 y3 = m_Broadphase.AabbSoA.AabbMin[m_Broadphase.BodiesAabbOffset + num12];
				float3 y4 = m_Broadphase.AabbSoA.AabbMax[m_Broadphase.BodiesAabbOffset + num12];
				int num13 = num12 * 16;
				for (int num14 = 0; num14 < 16; num14++)
				{
					int index5 = num14 + num13;
					int num15 = m_Broadphase.BodyCollidersPairsSoA.Array[index5];
					if (num15 <= -1)
					{
						break;
					}
					num15 += m_Broadphase.CollidersAabbOffset;
					float3 x5 = m_Broadphase.AabbSoA.AabbMin[num15];
					float3 x6 = m_Broadphase.AabbSoA.AabbMax[num15];
					x5 = math.min(x5, y3);
					x6 = math.max(x6, y4);
					float3 float17 = x6 - x5;
					float3 float18 = (x5 + x6) * 0.5f;
					Gizmos.color = PBD.DebugSettings.BodyColliderPairColor;
					Gizmos.DrawWireCube(float18, float17);
				}
				int num16 = num12 * 8;
				for (int num17 = 0; num17 < 8; num17++)
				{
					int index6 = num17 + num16;
					int num18 = m_Broadphase.BodyForceVolumePairsSoA.Array[index6];
					if (num18 <= -1)
					{
						break;
					}
					num18 += m_Broadphase.ForceVolumesAabbOffset;
					float3 x7 = m_Broadphase.AabbSoA.AabbMin[num18];
					float3 x8 = m_Broadphase.AabbSoA.AabbMax[num18];
					x7 = math.min(x7, y3);
					x8 = math.max(x8, y4);
					float3 float19 = x8 - x7;
					float3 float20 = (x7 + x8) * 0.5f;
					Gizmos.color = PBD.DebugSettings.BodyForceVolumePairColor;
					Gizmos.DrawWireCube(float20, float19);
				}
			}
			break;
		}
		case BroadphaseType.OptimizedSpatialHashing:
		{
			_ = m_Broadphase;
			for (int i = 0; i < m_MemoryManager.BodiesCount; i++)
			{
				float3 y = m_Broadphase.AabbSoA.AabbMin[m_Broadphase.BodiesAabbOffset + i];
				float3 y2 = m_Broadphase.AabbSoA.AabbMax[m_Broadphase.BodiesAabbOffset + i];
				int num = i * 16;
				for (int j = 0; j < 16; j++)
				{
					int index = j + num;
					int num2 = m_Broadphase.BodyCollidersPairsSoA.Array[index];
					if (num2 <= -1)
					{
						break;
					}
					num2 += m_Broadphase.CollidersAabbOffset;
					float3 x = m_Broadphase.AabbSoA.AabbMin[num2];
					float3 x2 = m_Broadphase.AabbSoA.AabbMax[num2];
					x = math.min(x, y);
					x2 = math.max(x2, y2);
					float3 @float = x2 - x;
					float3 float2 = (x + x2) * 0.5f;
					Gizmos.color = PBD.DebugSettings.BodyColliderPairColor;
					Gizmos.DrawWireCube(float2, @float);
				}
				int num3 = i * 8;
				for (int k = 0; k < 8; k++)
				{
					int index2 = k + num3;
					int num4 = m_Broadphase.BodyForceVolumePairsSoA.Array[index2];
					if (num4 <= -1)
					{
						break;
					}
					num4 += m_Broadphase.ForceVolumesAabbOffset;
					float3 x3 = m_Broadphase.AabbSoA.AabbMin[num4];
					float3 x4 = m_Broadphase.AabbSoA.AabbMax[num4];
					x3 = math.min(x3, y);
					x4 = math.max(x4, y2);
					float3 float3 = x4 - x3;
					float3 float4 = (x3 + x4) * 0.5f;
					Gizmos.color = PBD.DebugSettings.BodyForceVolumePairColor;
					Gizmos.DrawWireCube(float4, float3);
				}
			}
			break;
		}
		}
		Gizmos.color = color;
	}

	internal void DrawBoundingBoxes()
	{
		Color color = Gizmos.color;
		int num = m_Broadphase.BodiesCount + m_Broadphase.CollidersCount + m_Broadphase.ForceVolumesCount;
		for (int i = 0; i < num; i++)
		{
			if (i >= m_Broadphase.CollidersAabbOffset && i < m_Broadphase.CollidersAabbOffset + m_Broadphase.CollidersCount)
			{
				Gizmos.color = PBD.DebugSettings.ColliderColor;
			}
			if (i >= m_Broadphase.ForceVolumesAabbOffset && i < m_Broadphase.ForceVolumesAabbOffset + m_Broadphase.ForceVolumesCount)
			{
				Gizmos.color = PBD.DebugSettings.ForceVolumeColor;
			}
			if (i >= m_Broadphase.BodiesAabbOffset && i < m_Broadphase.BodiesAabbOffset + m_Broadphase.BodiesCount)
			{
				Gizmos.color = PBD.DebugSettings.BodyColor;
			}
			float3 @float = m_Broadphase.AabbSoA.AabbMin[i];
			float3 float2 = m_Broadphase.AabbSoA.AabbMax[i];
			float3 float3 = float2 - @float;
			Gizmos.DrawWireCube((@float + float2) * 0.5f, float3);
		}
		Gizmos.color = color;
	}

	public static int DivRoundUp(int x, int y)
	{
		return (x + y - 1) / y;
	}

	public void Simulate()
	{
		m_MemoryManager.Update();
		if (!m_Gpu && !IsEmpty && (PBD.WillSimulateOnCurrentFrame || PBD.DebugSettings.UpdateEveryFrame))
		{
			Tick(PBD.UpdatePeriod, default(JobHandle)).Complete();
			if (PBD.OnAfterUpdate != null)
			{
				PBD.OnAfterUpdate();
			}
		}
	}

	private JobHandle Tick(float updatePeriod, JobHandle lastJobHandle)
	{
		if (m_Broadphase.Type != PBD.BroadphaseSettings.Type)
		{
			ResetBroadphase();
		}
		m_Broadphase.ResizeBuffers(m_MemoryManager.GlobalCollidersCount, m_MemoryManager.BodiesCount, m_MemoryManager.ForceVolumesCount);
		lastJobHandle = UpdateColliders(lastJobHandle);
		lastJobHandle = UpdateForceVolumes(lastJobHandle);
		lastJobHandle = UpdateParticles(lastJobHandle);
		lastJobHandle = UpdateBodyAabb(lastJobHandle);
		lastJobHandle = m_Broadphase.Update(lastJobHandle);
		lastJobHandle = SimulatePhysics(updatePeriod, lastJobHandle);
		lastJobHandle = UpdateMeshAfterSimulation(lastJobHandle);
		return lastJobHandle;
	}

	private void ResetBroadphase()
	{
		if (m_Broadphase != null)
		{
			m_Broadphase.Dispose();
		}
		switch (PBD.BroadphaseSettings.Type)
		{
		case BroadphaseType.SimpleGrid:
			m_Broadphase = new SimpleGridBroadphase(PBD.BroadphaseSettings.SimpleGridSettings);
			break;
		case BroadphaseType.MultilevelGrid:
			m_Broadphase = new MultilevelGridBroadphase(PBD.BroadphaseSettings.MultilevelGridSettings);
			break;
		case BroadphaseType.OptimizedSpatialHashing:
			m_Broadphase = new OptimizedSpatialHashingBroadphase(PBD.BroadphaseSettings.OptimizedSpatialHashingSettings);
			break;
		}
	}

	private JobHandle UpdateColliders(JobHandle lastJobHandle)
	{
		if (m_MemoryManager.ColliderUpdateBuffers == null)
		{
			m_MemoryManager.ColliderUpdateBuffers = new ColliderUpdateBuffersSoA();
		}
		if (m_MemoryManager.Colliders == null)
		{
			m_MemoryManager.Colliders = new ColliderSoA(m_MemoryManager.CollidersRefs.Count);
		}
		else if (m_MemoryManager.Colliders.Length < m_MemoryManager.CollidersRefs.Count)
		{
			m_MemoryManager.Colliders.Resize((int)((float)m_MemoryManager.CollidersRefs.Count * 1.5f));
		}
		for (int i = 0; i < m_MemoryManager.CollidersRefs.Count; i += 256)
		{
			int num = math.min(256, m_MemoryManager.CollidersRefs.Count - i);
			for (int j = 0; j < num; j++)
			{
				ColliderRef colliderRef = m_MemoryManager.CollidersRefs[j + i];
				m_MemoryManager.ColliderUpdateBuffers.Type[j] = (int)colliderRef.Type;
				m_MemoryManager.ColliderUpdateBuffers.ObjectToWorld[j] = colliderRef.World;
				m_MemoryManager.ColliderUpdateBuffers.Parameters0[j] = colliderRef.Parameters0;
				m_MemoryManager.ColliderUpdateBuffers.MaterialParameters[j] = new float4(colliderRef.Restitution, colliderRef.Friction, 0f, 0f);
			}
			UpdateCollidersJob jobData = default(UpdateCollidersJob);
			jobData.Type = m_MemoryManager.ColliderUpdateBuffers.Type;
			jobData.Matrices = m_MemoryManager.ColliderUpdateBuffers.ObjectToWorld;
			jobData.Parameters0 = m_MemoryManager.ColliderUpdateBuffers.Parameters0;
			jobData.MaterialParametersIn = m_MemoryManager.ColliderUpdateBuffers.MaterialParameters;
			jobData.ColliderTypeList = m_MemoryManager.Colliders.Type;
			jobData.ColliderParameters0 = m_MemoryManager.Colliders.Parameters0;
			jobData.ColliderParameters1 = m_MemoryManager.Colliders.Parameters1;
			jobData.ColliderParameters2 = m_MemoryManager.Colliders.Parameters2;
			jobData.MaterialParameters = m_MemoryManager.Colliders.MaterialParameters;
			jobData.ColliderAabbMin = m_Broadphase.AabbSoA.AabbMin;
			jobData.ColliderAabbMax = m_Broadphase.AabbSoA.AabbMax;
			jobData.BatchOffset = i;
			lastJobHandle = IJobParallelForExtensions.Schedule(jobData, num, 1, lastJobHandle);
			lastJobHandle.Complete();
		}
		return lastJobHandle;
	}

	private JobHandle UpdateForceVolumes(JobHandle lastJobHandle)
	{
		if (m_MemoryManager.ForceVolumeUpdateBuffersSoA == null)
		{
			m_MemoryManager.ForceVolumeUpdateBuffersSoA = new ForceVolumeUpdateBuffersSoA();
		}
		if (m_MemoryManager.ForceVolumesSoA == null)
		{
			m_MemoryManager.ForceVolumesSoA = new ForceVolumeSoA(m_MemoryManager.ForceVolumes.Count);
		}
		else if (m_MemoryManager.ForceVolumesSoA.Length < m_MemoryManager.ForceVolumes.Count)
		{
			m_MemoryManager.ForceVolumesSoA.Resize((int)((float)m_MemoryManager.ForceVolumes.Count * 1.5f));
		}
		for (int i = 0; i < m_MemoryManager.ForceVolumes.Count; i += 256)
		{
			int num = math.min(256, m_MemoryManager.ForceVolumes.Count - i);
			for (int j = 0; j < num; j++)
			{
				ForceVolume forceVolume = m_MemoryManager.ForceVolumes[j + i];
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.EnumPackedValues[j] = ForceVolume.GetPackedEnumValues(forceVolume);
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.LocalToWorldVolumeMatrices[j] = forceVolume.LocalToWorldVolume;
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.VolumeParameters0[j] = forceVolume.VolumeParameters;
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.LocalToWorldEmitterMatrices[j] = forceVolume.LocalToWorldEmitter;
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.EmitterDirection[j] = forceVolume.Direction;
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.EmitterDirectionLerp[j] = forceVolume.DirectionLerp;
				m_MemoryManager.ForceVolumeUpdateBuffersSoA.EmitterIntensity[j] = forceVolume.Intensity;
			}
			UpdateForceVolumesJob jobData = default(UpdateForceVolumesJob);
			jobData.EnumPackedValues = m_MemoryManager.ForceVolumeUpdateBuffersSoA.EnumPackedValues;
			jobData.LocalToWorldVolumeMatrices = m_MemoryManager.ForceVolumeUpdateBuffersSoA.LocalToWorldVolumeMatrices;
			jobData.VolumeParameters0 = m_MemoryManager.ForceVolumeUpdateBuffersSoA.VolumeParameters0;
			jobData.LocalToWorldEmitterMatrices = m_MemoryManager.ForceVolumeUpdateBuffersSoA.LocalToWorldEmitterMatrices;
			jobData.EmitterDirection = m_MemoryManager.ForceVolumeUpdateBuffersSoA.EmitterDirection;
			jobData.EmitterDirectionLerp = m_MemoryManager.ForceVolumeUpdateBuffersSoA.EmitterDirectionLerp;
			jobData.EmitterIntensity = m_MemoryManager.ForceVolumeUpdateBuffersSoA.EmitterIntensity;
			jobData.ForceVolumeEnumPackedValues = m_MemoryManager.ForceVolumesSoA.EnumPackedValues;
			jobData.ForceVolumeParameters = m_MemoryManager.ForceVolumesSoA.VolumeParameters;
			jobData.ForceVolumeAabbMin = m_Broadphase.AabbSoA.AabbMin;
			jobData.ForceVolumeAabbMax = m_Broadphase.AabbSoA.AabbMax;
			jobData.AabbOffset = m_Broadphase.ForceVolumesAabbOffset;
			jobData.ForceVolumeEmissionParameters = m_MemoryManager.ForceVolumesSoA.EmissionParameters;
			jobData.BatchOffset = i;
			lastJobHandle = IJobParallelForExtensions.Schedule(jobData, num, 1, lastJobHandle);
		}
		return lastJobHandle;
	}

	private JobHandle UpdateParticles(JobHandle lastJobHandle)
	{
		if (m_MemoryManager.BodyUpdateBuffersSoA == null)
		{
			m_MemoryManager.BodyUpdateBuffersSoA = new BodyUpdateBuffersSoA();
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<SkinnedBody, int> item in m_MemoryManager.SkinnedBodyDescriptorsMap)
		{
			m_MemoryManager.BodyUpdateBuffersSoA.BoneUpdateParticleMatrices[num] = item.Key.LocalToWorld;
			num++;
			if (num >= 256 || num + num2 >= m_MemoryManager.SkinnedBodyDescriptorsMap.Count)
			{
				UpdateSkinnedParticlesJob jobData = default(UpdateSkinnedParticlesJob);
				jobData.Offset = num2;
				jobData.SkinnedBodyIndices = m_MemoryManager.SkinnedBodyDescriptorsIndicesSoA.Array;
				jobData.ParticlesOffsetCount = m_MemoryManager.BodyDescriptorsSoA.ParticlesOffsetCount;
				jobData.BonesOffset = m_MemoryManager.BodyDescriptorsSoA.SkinnedDataOffset;
				jobData.BonesCount = m_MemoryManager.BodyDescriptorsSoA.SkinnedDataCount;
				jobData.TeleportDistanceTreshold = m_MemoryManager.BodyDescriptorsSoA.TeleportDistanceTreshold;
				jobData.LocalToWorldMatrices = m_MemoryManager.BodyUpdateBuffersSoA.BoneUpdateParticleMatrices;
				jobData.Boneposes = m_MemoryManager.SkinnedDataSoA.Boneposes;
				jobData.BasePosition = m_MemoryManager.ParticlesSoA.BasePosition;
				jobData.Position = m_MemoryManager.ParticlesSoA.Position;
				jobData.Mass = m_MemoryManager.ParticlesSoA.Mass;
				lastJobHandle = IJobParallelForExtensions.Schedule(jobData, num, 1, lastJobHandle);
				lastJobHandle.Complete();
				num2 += math.min(num, 256);
				num = 0;
			}
		}
		num = 0;
		num2 = 0;
		foreach (KeyValuePair<MeshBody, int> item2 in m_MemoryManager.MeshBodyDescriptorsMap)
		{
			m_MemoryManager.BodyUpdateBuffersSoA.VertexUpdateParticleMatrices[num] = item2.Key.LocalToWorld;
			num++;
			if (num >= 256 || num + num2 >= m_MemoryManager.MeshBodyDescriptorsMap.Count)
			{
				UpdateMeshParticlesJob jobData2 = default(UpdateMeshParticlesJob);
				jobData2.Offset = num2;
				jobData2.MeshBodyIndices = m_MemoryManager.MeshBodyDescriptorsIndicesSoA.Array;
				jobData2.ParticlesOffsetCount = m_MemoryManager.BodyDescriptorsSoA.ParticlesOffsetCount;
				jobData2.VerticesOffset = m_MemoryManager.BodyDescriptorsSoA.VerticesOffset;
				jobData2.VerticesCount = m_MemoryManager.BodyDescriptorsSoA.VerticesCount;
				jobData2.TeleportDistanceTreshold = m_MemoryManager.BodyDescriptorsSoA.TeleportDistanceTreshold;
				jobData2.LocalToWorldMatrices = m_MemoryManager.BodyUpdateBuffersSoA.VertexUpdateParticleMatrices;
				jobData2.BaseVertices = m_MemoryManager.MeshBodyVerticesSoA.BaseVertices;
				jobData2.BasePosition = m_MemoryManager.ParticlesSoA.BasePosition;
				jobData2.Position = m_MemoryManager.ParticlesSoA.Position;
				jobData2.Mass = m_MemoryManager.ParticlesSoA.Mass;
				lastJobHandle = IJobParallelForExtensions.Schedule(jobData2, num, 1, lastJobHandle);
				lastJobHandle.Complete();
				num2 += math.min(num, 256);
				num = 0;
			}
		}
		return lastJobHandle;
	}

	private JobHandle UpdateBodyAabb(JobHandle lastJobHandle)
	{
		UpdateBodyAabbJob jobData = default(UpdateBodyAabbJob);
		jobData.BodiesAabbOffset = m_Broadphase.BodiesAabbOffset;
		jobData.BodyDescriptorsIndices = m_MemoryManager.BodyDescriptorsIndicesSoA.Array;
		jobData.ParticlesOffsetCount = m_MemoryManager.BodyDescriptorsSoA.ParticlesOffsetCount;
		jobData.Position = m_MemoryManager.ParticlesSoA.Position;
		jobData.BasePosition = m_MemoryManager.ParticlesSoA.BasePosition;
		jobData.Radius = m_MemoryManager.ParticlesSoA.Radius;
		jobData.AabbMin = m_Broadphase.AabbSoA.AabbMin;
		jobData.AabbMax = m_Broadphase.AabbSoA.AabbMax;
		lastJobHandle = IJobParallelForExtensions.Schedule(jobData, m_MemoryManager.BodyDescriptorsMap.Count, 1, lastJobHandle);
		return lastJobHandle;
	}

	private JobHandle SimulatePhysics(float updatePeriod, JobHandle lastJobHandle)
	{
		SimulationJob simulationJob = default(SimulationJob);
		simulationJob.Decay = m_Decay;
		simulationJob.DeltaTime = updatePeriod;
		simulationJob.SimulationIterations = m_SimulationIterations;
		simulationJob.ConstraintIterations = m_ConstraintIterations;
		simulationJob.VelocitySleepThreshold = 1f;
		simulationJob.UseExperimentalFeatures = PBD.UseExperimentalFeatures;
		simulationJob.BodyDescriptorsIndices = m_MemoryManager.BodyDescriptorsIndicesSoA.Array;
		simulationJob.ParticlesOffsetCount = m_MemoryManager.BodyDescriptorsSoA.ParticlesOffsetCount;
		simulationJob.ConstraintsOffsetCount = m_MemoryManager.BodyDescriptorsSoA.ConstraintsOffsetCount;
		simulationJob.LocalCollidersOffsetCount = m_MemoryManager.BodyDescriptorsSoA.LocalCollidersOffsetCount;
		simulationJob.MaterialParameters = m_MemoryManager.BodyDescriptorsSoA.MaterialParameters;
		simulationJob.BasePosition = m_MemoryManager.ParticlesSoA.BasePosition;
		simulationJob.Position = m_MemoryManager.ParticlesSoA.Position;
		simulationJob.Predicted = m_MemoryManager.ParticlesSoA.Predicted;
		simulationJob.Velocity = m_MemoryManager.ParticlesSoA.Velocity;
		simulationJob.Orientation = m_MemoryManager.ParticlesSoA.Orientation;
		simulationJob.PredictedOrientation = m_MemoryManager.ParticlesSoA.PredictedOrientation;
		simulationJob.AngularVelocity = m_MemoryManager.ParticlesSoA.AngularVelocity;
		simulationJob.Mass = m_MemoryManager.ParticlesSoA.Mass;
		simulationJob.Radius = m_MemoryManager.ParticlesSoA.Radius;
		simulationJob.Flags = m_MemoryManager.ParticlesSoA.Flags;
		simulationJob.Index0 = m_MemoryManager.ConstraintsSoA.Index0;
		simulationJob.Index1 = m_MemoryManager.ConstraintsSoA.Index1;
		simulationJob.Index2 = m_MemoryManager.ConstraintsSoA.Index2;
		simulationJob.Index3 = m_MemoryManager.ConstraintsSoA.Index3;
		simulationJob.Parameters0 = m_MemoryManager.ConstraintsSoA.Parameters0;
		simulationJob.Parameters1 = m_MemoryManager.ConstraintsSoA.Parameters1;
		simulationJob.Type = m_MemoryManager.ConstraintsSoA.Type;
		simulationJob.ColliderParameters0 = m_MemoryManager.Colliders.Parameters0;
		simulationJob.ColliderParameters1 = m_MemoryManager.Colliders.Parameters1;
		simulationJob.ColliderParameters2 = m_MemoryManager.Colliders.Parameters2;
		simulationJob.ColliderMaterialParameters = m_MemoryManager.Colliders.MaterialParameters;
		simulationJob.ColliderTypeList = m_MemoryManager.Colliders.Type;
		simulationJob.ForceVolumeEnumPackedValues = m_MemoryManager.ForceVolumesSoA.EnumPackedValues;
		simulationJob.ForceVolumeParameters = m_MemoryManager.ForceVolumesSoA.VolumeParameters;
		simulationJob.ForceVolumeEmissionParameters = m_MemoryManager.ForceVolumesSoA.EmissionParameters;
		simulationJob.BroadphaseType = PBD.BroadphaseSettings.Type;
		simulationJob.BroadphaseSceneAabb = m_Broadphase.SceneAabb;
		simulationJob.BodyColliderPairs = m_Broadphase.BodyCollidersPairsSoA.Array;
		simulationJob.BodyForceVolumePairs = m_Broadphase.BodyForceVolumePairsSoA.Array;
		simulationJob.BroadphaseGridResolution = PBD.BroadphaseSettings.SimpleGridSettings.GridResolution;
		simulationJob.WindVector = default(float2);
		simulationJob.Gravity = default(float3);
		simulationJob.StrengthNoiseWeight = 0f;
		simulationJob.CompressedStrengthOctaves = m_MemoryManager.DefaultWindData;
		simulationJob.CompressedShiftOctaves = m_MemoryManager.DefaultWindData;
		SimulationJob job = simulationJob;
		foreach (IForce force in m_MemoryManager.Forces)
		{
			force.SetupSimulationJob(ref job);
		}
		lastJobHandle = IJobParallelForExtensions.Schedule(job, m_MemoryManager.BodyDescriptorsMap.Count, 1, lastJobHandle);
		return lastJobHandle;
	}

	private JobHandle UpdateMeshAfterSimulation(JobHandle lastJobHandle)
	{
		if (m_MemoryManager.BodyUpdateBuffersSoA == null)
		{
			m_MemoryManager.BodyUpdateBuffersSoA = new BodyUpdateBuffersSoA();
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<MeshBody, int> item in m_MemoryManager.MeshBodyDescriptorsMap)
		{
			m_MemoryManager.BodyUpdateBuffersSoA.ParticleUpdateVertexMatrices[num] = item.Key.LocalToWorld.inverse;
			num++;
			if (num >= 256 || num >= m_MemoryManager.MeshBodyDescriptorsMap.Count)
			{
				UpdateMeshAfterSimulationJob jobData = default(UpdateMeshAfterSimulationJob);
				jobData.Offset = num2;
				jobData.MeshBodyIndices = m_MemoryManager.MeshBodyDescriptorsIndicesSoA.Array;
				jobData.IndicesOffset = m_MemoryManager.BodyDescriptorsSoA.IndicesOffset;
				jobData.IndicesCount = m_MemoryManager.BodyDescriptorsSoA.IndicesCount;
				jobData.ParticlesOffsetCount = m_MemoryManager.BodyDescriptorsSoA.ParticlesOffsetCount;
				jobData.VertexTriangleMapOffset = m_MemoryManager.BodyDescriptorsSoA.VertexTriangleMapOffset;
				jobData.VertexTriangleMapOffsetCountOffset = m_MemoryManager.BodyDescriptorsSoA.VertexTriangleMapOffsetCountOffset;
				jobData.VerticesOffset = m_MemoryManager.BodyDescriptorsSoA.VerticesOffset;
				jobData.VerticesCount = m_MemoryManager.BodyDescriptorsSoA.VerticesCount;
				jobData.WorldToLocalMatrices = m_MemoryManager.BodyUpdateBuffersSoA.ParticleUpdateVertexMatrices;
				jobData.Position = m_MemoryManager.ParticlesSoA.Position;
				jobData.Indices = m_MemoryManager.MeshBodyIndicesSoA.Array;
				jobData.Vertices = m_MemoryManager.MeshBodyVerticesSoA.Vertices;
				jobData.Normals = m_MemoryManager.MeshBodyVerticesSoA.Normals;
				jobData.Tangents = m_MemoryManager.MeshBodyVerticesSoA.Tangents;
				jobData.Uvs = m_MemoryManager.MeshBodyVerticesSoA.Uvs;
				jobData.VertexTriangleMap = m_MemoryManager.MeshBodyVertexTriangleMapSoA.Array;
				jobData.VertexTriangleMapOffsetCount = m_MemoryManager.MeshBodyVertexTriangleMapOffsetCountSoA.Array;
				lastJobHandle = IJobParallelForExtensions.Schedule(jobData, num, 1, lastJobHandle);
				num2 += num;
				num = 0;
			}
		}
		return lastJobHandle;
	}
}
