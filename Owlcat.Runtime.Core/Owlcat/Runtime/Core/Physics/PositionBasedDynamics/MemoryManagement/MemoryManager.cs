using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Constraints;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Particles;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;

internal class MemoryManager
{
	private bool m_Gpu;

	public GPUData GpuData;

	private HashSet<Body> m_AddedBodies = new HashSet<Body>();

	private HashSet<Body> m_RemovedBodies = new HashSet<Body>();

	private HashSet<Body> m_DirtyDataBodies = new HashSet<Body>();

	private Dictionary<BodyGroups, List<Body>> m_BodyGroups = new Dictionary<BodyGroups, List<Body>>
	{
		{
			BodyGroups.GroupGrass,
			new List<Body>()
		},
		{
			BodyGroups.Group32,
			new List<Body>()
		},
		{
			BodyGroups.Group64,
			new List<Body>()
		},
		{
			BodyGroups.Group128,
			new List<Body>()
		},
		{
			BodyGroups.Group192,
			new List<Body>()
		},
		{
			BodyGroups.Group256,
			new List<Body>()
		},
		{
			BodyGroups.Group512,
			new List<Body>()
		}
	};

	private Dictionary<BodyGroups, int2> m_BodyGroupsOffsetCount = new Dictionary<BodyGroups, int2>
	{
		{
			BodyGroups.GroupGrass,
			default(int2)
		},
		{
			BodyGroups.Group32,
			default(int2)
		},
		{
			BodyGroups.Group64,
			default(int2)
		},
		{
			BodyGroups.Group128,
			default(int2)
		},
		{
			BodyGroups.Group192,
			default(int2)
		},
		{
			BodyGroups.Group256,
			default(int2)
		},
		{
			BodyGroups.Group512,
			default(int2)
		}
	};

	private Dictionary<Body, int> m_BodyDescriptorsMap = new Dictionary<Body, int>();

	private Dictionary<SkinnedBody, int> m_SkinnedBodyDescriptorsMap = new Dictionary<SkinnedBody, int>();

	private Dictionary<MeshBody, int> m_MeshBodyDescriptorsMap = new Dictionary<MeshBody, int>();

	private bool m_IsBodiesDirty;

	private bool m_IsReset;

	private Dictionary<int, int> m_BodySizeGroups = new Dictionary<int, int>();

	public BodyDescriptorSoA BodyDescriptorsSoA = new BodyDescriptorSoA();

	public SkinnedBodySoA SkinnedDataSoA = new SkinnedBodySoA();

	public SingleArraySoA<int> SkinnedBodyBoneIndicesMapSoA = new SingleArraySoA<int>();

	public MeshBodyVerticesSoA MeshBodyVerticesSoA = new MeshBodyVerticesSoA();

	public SingleArraySoA<int> MeshBodyIndicesSoA = new SingleArraySoA<int>();

	public SingleArraySoA<int> MeshBodyVertexTriangleMapSoA = new SingleArraySoA<int>();

	public SingleArraySoA<int> MeshBodyVertexTriangleMapOffsetCountSoA = new SingleArraySoA<int>();

	public BodyUpdateBuffersSoA BodyUpdateBuffersSoA;

	public SingleArraySoA<int> BodyDescriptorsIndicesSoA = new SingleArraySoA<int>();

	public SingleArraySoA<int> SkinnedBodyDescriptorsIndicesSoA = new SingleArraySoA<int>();

	public SingleArraySoA<int> MeshBodyDescriptorsIndicesSoA = new SingleArraySoA<int>();

	public int ParticlesCount;

	public ParticleSoA ParticlesSoA = new ParticleSoA();

	public int ConstraintsCount;

	public ConstraintSoA ConstraintsSoA = new ConstraintSoA();

	public SingleArraySoA<int2> DisconnectedConstraintsGroupsSoA = new SingleArraySoA<int2>(64);

	private bool m_IsColliderBodyDependencyDirty;

	private bool m_IsCollidersDirty;

	public int GlobalCollidersCount;

	public List<ColliderRef> CollidersRefs = new List<ColliderRef>();

	public ColliderSoA Colliders;

	public ColliderUpdateBuffersSoA ColliderUpdateBuffers;

	private bool m_IsForceVolumesDirty;

	public HashSet<IForce> Forces = new HashSet<IForce>();

	public NativeArray<float4> DefaultWindData;

	public ForceVolumeSoA ForceVolumesSoA;

	public ForceVolumeUpdateBuffersSoA ForceVolumeUpdateBuffersSoA;

	public List<ForceVolume> ForceVolumes = new List<ForceVolume>();

	private int m_SceneInitializationCounter;

	public bool IsEmpty
	{
		get
		{
			if (m_BodyDescriptorsMap.Count <= 0)
			{
				return m_AddedBodies.Count <= 0;
			}
			return false;
		}
	}

	public int BodiesCount => m_BodyDescriptorsMap.Count;

	public int CollidersCount => CollidersRefs.Count;

	public int ForceVolumesCount => ForceVolumes.Count;

	public Dictionary<Body, int> BodyDescriptorsMap => m_BodyDescriptorsMap;

	public Dictionary<SkinnedBody, int> SkinnedBodyDescriptorsMap => m_SkinnedBodyDescriptorsMap;

	public Dictionary<MeshBody, int> MeshBodyDescriptorsMap => m_MeshBodyDescriptorsMap;

	public bool IsSceneInitialization => m_SceneInitializationCounter > 0;

	public MemoryManager(bool gpu)
	{
		m_Gpu = gpu;
		if (m_Gpu)
		{
			GpuData = new GPUData();
		}
		else
		{
			DefaultWindData = new NativeArray<float4>(4, Allocator.Persistent);
		}
	}

	public void Dispose()
	{
		if (m_Gpu)
		{
			GpuData.Dispose();
		}
		BodyDescriptorsSoA.Dispose();
		ParticlesSoA.Dispose();
		ConstraintsSoA.Dispose();
		DisconnectedConstraintsGroupsSoA.Dispose();
		SkinnedDataSoA.Dispose();
		SkinnedBodyBoneIndicesMapSoA.Dispose();
		MeshBodyVerticesSoA.Dispose();
		MeshBodyIndicesSoA.Dispose();
		MeshBodyVertexTriangleMapSoA.Dispose();
		MeshBodyVertexTriangleMapOffsetCountSoA.Dispose();
		BodyDescriptorsIndicesSoA.Dispose();
		SkinnedBodyDescriptorsIndicesSoA.Dispose();
		MeshBodyDescriptorsIndicesSoA.Dispose();
		if (BodyUpdateBuffersSoA != null)
		{
			BodyUpdateBuffersSoA.Dispose();
		}
		if (Colliders != null)
		{
			Colliders.Dispose();
		}
		if (ColliderUpdateBuffers != null)
		{
			ColliderUpdateBuffers.Dispose();
		}
		if (!m_Gpu && DefaultWindData.IsCreated)
		{
			DefaultWindData.Dispose();
		}
		if (ForceVolumesSoA != null)
		{
			ForceVolumesSoA.Dispose();
		}
		if (ForceVolumeUpdateBuffersSoA != null)
		{
			ForceVolumeUpdateBuffersSoA.Dispose();
		}
	}

	internal void BeginSceneInitialization()
	{
		m_SceneInitializationCounter++;
	}

	internal void EndSceneInitialization()
	{
		if (m_SceneInitializationCounter <= 0)
		{
			Debug.LogError("PBD scene initialization is not beginned but you are trying to stop it.");
		}
		m_SceneInitializationCounter--;
		m_SceneInitializationCounter = ((m_SceneInitializationCounter >= 0) ? m_SceneInitializationCounter : 0);
		if (m_SceneInitializationCounter == 0)
		{
			Reset();
			PBD.OnSceneInitializationFinished?.Invoke();
		}
	}

	internal void Reset()
	{
		m_IsReset = true;
	}

	internal void Update()
	{
		if (m_IsBodiesDirty)
		{
			BuildBodies();
			m_IsColliderBodyDependencyDirty = true;
			m_IsBodiesDirty = false;
		}
		if (m_IsCollidersDirty)
		{
			BuildColliders();
			m_IsColliderBodyDependencyDirty = true;
			m_IsCollidersDirty = false;
		}
		if (m_IsColliderBodyDependencyDirty)
		{
			BuildBodyColliderDependencies();
			m_IsColliderBodyDependencyDirty = false;
		}
		if (m_IsForceVolumesDirty)
		{
			BuildForceVolumes();
			m_IsForceVolumesDirty = false;
		}
	}

	internal void BuildBodies()
	{
		if (!m_IsBodiesDirty)
		{
			return;
		}
		if (m_SceneInitializationCounter > 0)
		{
			Debug.LogError("Attempt to BuildBodies while scene initialization");
		}
		if (m_RemovedBodies.Any((Body b) => m_AddedBodies.Contains(b)))
		{
			Debug.LogError("Attempt to remove not added bodies");
		}
		if (!m_IsReset)
		{
			if (m_BodyDescriptorsMap.Count == 0 || m_BodyDescriptorsMap.Count == m_RemovedBodies.Count)
			{
				m_IsReset = true;
			}
			if (m_AddedBodies.Any((Body b) => b is GrassBody))
			{
				m_IsReset = true;
			}
			if (m_RemovedBodies.Any((Body b) => b is GrassBody))
			{
				m_IsReset = true;
			}
		}
		if (m_RemovedBodies.Count > 0)
		{
			RemoveBodies();
		}
		if (!m_IsReset && m_AddedBodies.Count > 0)
		{
			m_IsReset = !TryToPlaceInMemory();
		}
		if (m_IsReset)
		{
			ResetMemory();
			if (m_AddedBodies.Count > 0 && !TryToPlaceInMemory())
			{
				Debug.LogError("Can't place bodies in memory");
			}
		}
		SetData();
		PBD.OnBodyDataUpdated?.Invoke(m_AddedBodies);
		m_AddedBodies.Clear();
		m_RemovedBodies.Clear();
		m_IsBodiesDirty = false;
		m_IsReset = false;
	}

	private void RemoveBodies()
	{
		foreach (Body removedBody in m_RemovedBodies)
		{
			int num = m_BodyDescriptorsMap[removedBody];
			m_BodyDescriptorsMap.Remove(removedBody);
			BodyDescriptor bodyDescriptor = BodyDescriptorsSoA[num];
			BodyDescriptorsSoA.Free(num, 1);
			ParticlesSoA.Free(bodyDescriptor.ParticlesOffsetCount.x, bodyDescriptor.ParticlesOffsetCount.y);
			ParticlesCount -= bodyDescriptor.ParticlesOffsetCount.y;
			ConstraintsSoA.Free(bodyDescriptor.ConstraintsOffsetCount.x, bodyDescriptor.ConstraintsOffsetCount.y);
			ConstraintsCount -= bodyDescriptor.ConstraintsOffsetCount.y;
			DisconnectedConstraintsGroupsSoA.Free(bodyDescriptor.ConstraintsGroupsOffsetCount.x, bodyDescriptor.ConstraintsGroupsOffsetCount.y);
			if (removedBody is SkinnedBody key)
			{
				m_SkinnedBodyDescriptorsMap.Remove(key);
				SkinnedDataSoA.Free(bodyDescriptor.SkinnedDataOffset, bodyDescriptor.SkinnedDataCount);
				SkinnedBodyBoneIndicesMapSoA.Free(bodyDescriptor.SkinnedBoneIndicesMapOffset, bodyDescriptor.SkinnedBoneIndicesMapCount);
			}
			if (removedBody is MeshBody key2)
			{
				m_MeshBodyDescriptorsMap.Remove(key2);
				MeshBodyIndicesSoA.Free(bodyDescriptor.IndicesOffset, bodyDescriptor.IndicesCount);
				MeshBodyVerticesSoA.Free(bodyDescriptor.VerticesOffset, bodyDescriptor.VerticesCount);
				MeshBodyVertexTriangleMapSoA.Free(bodyDescriptor.VertexTriangleMapOffset, bodyDescriptor.VertexTriangleMapCount);
				MeshBodyVertexTriangleMapOffsetCountSoA.Free(bodyDescriptor.VertexTriangleMapOffsetCountOffset, bodyDescriptor.VertexTriangleMapOffsetCountCount);
			}
		}
		if (m_Gpu)
		{
			GpuData.Version++;
		}
	}

	private bool TryToPlaceInMemory()
	{
		IEnumerable<Body> enumerable = m_AddedBodies;
		if (m_IsReset)
		{
			enumerable = m_AddedBodies.OrderBy((Body b) => (b is GrassBody) ? (-1) : b.Particles.Count);
		}
		foreach (Body item in enumerable)
		{
			if (!BodyDescriptorsSoA.TryAlloc(1, out var offset))
			{
				if (m_IsReset)
				{
					Debug.LogError("Can't alloc BodyDescriptor");
				}
				return false;
			}
			BodyDescriptor value = default(BodyDescriptor);
			value.MaterialParameters = new float2(item.Friction, item.Restitution);
			value.TeleportDistanceTreshold = item.TeleportDistanceTreshold;
			if (item.Particles.Count <= ParticlesSoA.Length && ParticlesSoA.TryAlloc(item.Particles.Count, out value.ParticlesOffsetCount.x))
			{
				value.ParticlesOffsetCount.y = item.Particles.Count;
				if (item.Constraints.Count <= ConstraintsSoA.Length && ConstraintsSoA.TryAlloc(item.Constraints.Count, out value.ConstraintsOffsetCount.x))
				{
					value.ConstraintsOffsetCount.y = item.Constraints.Count;
					if (item.DisconnectedConstraintsOffsetCount.Count <= DisconnectedConstraintsGroupsSoA.Length && DisconnectedConstraintsGroupsSoA.TryAlloc(item.DisconnectedConstraintsOffsetCount.Count, out value.ConstraintsGroupsOffsetCount.x))
					{
						value.ConstraintsGroupsOffsetCount.y = item.DisconnectedConstraintsOffsetCount.Count;
						if (item is SkinnedBody skinnedBody)
						{
							if (skinnedBody.Bindposes.Length > SkinnedDataSoA.Length || !SkinnedDataSoA.TryAlloc(skinnedBody.Bindposes.Length, out value.SkinnedDataOffset))
							{
								if (m_IsReset)
								{
									Debug.LogError("Can't alloc SkinnedData");
								}
								return false;
							}
							value.SkinnedDataCount = skinnedBody.Bindposes.Length;
							skinnedBody.BonesOffset = value.SkinnedDataOffset;
							if (skinnedBody.BoneIndicesMap.Length > SkinnedBodyBoneIndicesMapSoA.Length || !SkinnedBodyBoneIndicesMapSoA.TryAlloc(skinnedBody.BoneIndicesMap.Length, out value.SkinnedBoneIndicesMapOffset))
							{
								if (m_IsReset)
								{
									Debug.LogError("Can't alloc SkinnedBodyBoneIndices");
								}
								return false;
							}
							value.SkinnedBoneIndicesMapCount = skinnedBody.BoneIndicesMap.Length;
							skinnedBody.BoneIndicesMapOffset = value.SkinnedBoneIndicesMapOffset;
							m_SkinnedBodyDescriptorsMap.Add(skinnedBody, offset);
						}
						if (item is MeshBody meshBody)
						{
							if (meshBody.Indices.Count > MeshBodyIndicesSoA.Length || !MeshBodyIndicesSoA.TryAlloc(meshBody.Indices.Count, out value.IndicesOffset))
							{
								if (m_IsReset)
								{
									Debug.LogError("Can't alloc MeshBodyIndices");
								}
								return false;
							}
							value.IndicesCount = meshBody.Indices.Count;
							if (meshBody.Vertices.Count > MeshBodyVerticesSoA.Length || !MeshBodyVerticesSoA.TryAlloc(meshBody.Vertices.Count, out value.VerticesOffset))
							{
								if (m_IsReset)
								{
									Debug.LogError("Can't alloc MeshBodyVertices");
								}
								return false;
							}
							value.VerticesCount = meshBody.Vertices.Count;
							meshBody.VertexOffset = value.VerticesOffset;
							meshBody.ParticlesOffset = value.ParticlesOffsetCount.x;
							if (meshBody.VertexTriangleMap.Count > MeshBodyVertexTriangleMapSoA.Length || !MeshBodyVertexTriangleMapSoA.TryAlloc(meshBody.VertexTriangleMap.Count, out value.VertexTriangleMapOffset))
							{
								if (m_IsReset)
								{
									Debug.LogError("Can't alloc VertexTriangleMap");
								}
								return false;
							}
							value.VertexTriangleMapCount = meshBody.VertexTriangleMap.Count;
							if (meshBody.VertexTriangleMapOffsetCount.Count > MeshBodyVertexTriangleMapOffsetCountSoA.Length || !MeshBodyVertexTriangleMapOffsetCountSoA.TryAlloc(meshBody.VertexTriangleMapOffsetCount.Count, out value.VertexTriangleMapOffsetCountOffset))
							{
								if (m_IsReset)
								{
									Debug.LogError("Can't alloc VertexTriangleMapOffsetCount");
								}
								return false;
							}
							value.VertexTriangleMapOffsetCountCount = meshBody.VertexTriangleMapOffsetCount.Count;
							m_MeshBodyDescriptorsMap.Add(meshBody, offset);
						}
						ParticlesCount += value.ParticlesOffsetCount.y;
						ConstraintsCount += value.ConstraintsOffsetCount.y;
						m_BodyDescriptorsMap.Add(item, offset);
						BodyDescriptorsSoA[offset] = value;
						continue;
					}
					if (m_IsReset)
					{
						Debug.LogError("Can't alloc ConstraintsGroups");
					}
					return false;
				}
				if (m_IsReset)
				{
					Debug.LogError("Can't alloc Constraints");
				}
				return false;
			}
			if (m_IsReset)
			{
				Debug.LogError("Can't alloc Particles");
			}
			return false;
		}
		return true;
	}

	private void ResetMemory()
	{
		m_AddedBodies.UnionWith(m_BodyDescriptorsMap.Keys);
		m_BodyDescriptorsMap.Clear();
		m_SkinnedBodyDescriptorsMap.Clear();
		m_MeshBodyDescriptorsMap.Clear();
		ParticlesCount = 0;
		ConstraintsCount = 0;
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		int num6 = 0;
		int num7 = 0;
		int num8 = 0;
		int num9 = 0;
		foreach (Body addedBody in m_AddedBodies)
		{
			num += addedBody.Particles.Count;
			num2 += addedBody.Constraints.Count;
			num3 += addedBody.DisconnectedConstraintsOffsetCount.Count;
			if (addedBody is SkinnedBody skinnedBody)
			{
				num4 += skinnedBody.Bindposes.Length;
				num5 += skinnedBody.BoneIndicesMap.Length;
			}
			if (addedBody is MeshBody meshBody)
			{
				num6 += meshBody.Indices.Count;
				num7 += meshBody.Vertices.Count;
				num8 += meshBody.VertexTriangleMap.Count;
				num9 += meshBody.VertexTriangleMapOffsetCount.Count;
			}
		}
		if (m_AddedBodies.Count > BodyDescriptorsSoA.Length)
		{
			BodyDescriptorsSoA.Resize((int)((float)m_AddedBodies.Count * 1.5f));
		}
		else
		{
			BodyDescriptorsSoA.Reset();
		}
		if (num > ParticlesSoA.Length)
		{
			ParticlesSoA.Resize((int)((float)num * 1.5f));
		}
		else
		{
			ParticlesSoA.Reset();
		}
		if (num2 > ConstraintsSoA.Length)
		{
			ConstraintsSoA.Resize((int)((float)num2 * 1.5f));
		}
		else
		{
			ConstraintsSoA.Reset();
		}
		if (num3 > DisconnectedConstraintsGroupsSoA.Length)
		{
			DisconnectedConstraintsGroupsSoA.Resize((int)((float)num3 * 1.5f));
		}
		else
		{
			DisconnectedConstraintsGroupsSoA.Reset();
		}
		if (num4 > SkinnedDataSoA.Length)
		{
			SkinnedDataSoA.Resize((int)((float)num4 * 1.5f));
		}
		else
		{
			SkinnedDataSoA.Reset();
		}
		if (num5 > SkinnedBodyBoneIndicesMapSoA.Length)
		{
			SkinnedBodyBoneIndicesMapSoA.Resize((int)((float)num5 * 1.5f));
		}
		else
		{
			SkinnedBodyBoneIndicesMapSoA.Reset();
		}
		if (num6 > MeshBodyIndicesSoA.Length)
		{
			MeshBodyIndicesSoA.Resize((int)((float)num6 * 1.5f));
		}
		else
		{
			MeshBodyIndicesSoA.Reset();
		}
		if (num7 > MeshBodyVerticesSoA.Length)
		{
			MeshBodyVerticesSoA.Resize((int)((float)num7 * 1.5f));
		}
		else
		{
			MeshBodyVerticesSoA.Reset();
		}
		if (num8 > MeshBodyVertexTriangleMapSoA.Length)
		{
			MeshBodyVertexTriangleMapSoA.Resize((int)((float)num8 * 1.5f));
		}
		else
		{
			MeshBodyVertexTriangleMapSoA.Reset();
		}
		if (num9 > MeshBodyVertexTriangleMapOffsetCountSoA.Length)
		{
			MeshBodyVertexTriangleMapOffsetCountSoA.Resize((int)((float)num9 * 1.5f));
		}
		else
		{
			MeshBodyVertexTriangleMapOffsetCountSoA.Reset();
		}
		if (m_Gpu)
		{
			if (GpuData.BodyDescriptorsSoA.Length != BodyDescriptorsSoA.Length)
			{
				GpuData.BodyDescriptorsSoA.Resize(BodyDescriptorsSoA.Length);
			}
			if (GpuData.ParticlesSoA.Length != ParticlesSoA.Length)
			{
				GpuData.ParticlesSoA.Resize(ParticlesSoA.Length);
			}
			if (GpuData.ConstraintsSoA.Length != ConstraintsSoA.Length)
			{
				GpuData.ConstraintsSoA.Resize(ConstraintsSoA.Length);
			}
			if (GpuData.DisconnectedConstraintsGroupsSoA.Length != DisconnectedConstraintsGroupsSoA.Length)
			{
				GpuData.DisconnectedConstraintsGroupsSoA.Resize(DisconnectedConstraintsGroupsSoA.Length);
			}
			if (GpuData.SkinnedBodySoA.Length != SkinnedDataSoA.Length)
			{
				GpuData.SkinnedBodySoA.Resize(SkinnedDataSoA.Length);
			}
			if (GpuData.SkinnedBodyBoneIndicesMapSoA.Length != SkinnedBodyBoneIndicesMapSoA.Length)
			{
				GpuData.SkinnedBodyBoneIndicesMapSoA.Resize(SkinnedBodyBoneIndicesMapSoA.Length);
			}
			if (GpuData.MeshBodyVerticesSoA.Length != MeshBodyVerticesSoA.Length)
			{
				GpuData.MeshBodyVerticesSoA.Resize(MeshBodyVerticesSoA.Length);
			}
			if (GpuData.MeshBodyIndicesSoA.Length != MeshBodyIndicesSoA.Length)
			{
				GpuData.MeshBodyIndicesSoA.Resize(MeshBodyIndicesSoA.Length);
			}
			if (GpuData.MeshBodyVertexTriangleMapSoA.Length != MeshBodyVertexTriangleMapSoA.Length)
			{
				GpuData.MeshBodyVertexTriangleMapSoA.Resize(MeshBodyVertexTriangleMapSoA.Length);
			}
			if (GpuData.MeshBodyVertexTriangleMapOffsetCountSoA.Length != MeshBodyVertexTriangleMapOffsetCountSoA.Length)
			{
				GpuData.MeshBodyVertexTriangleMapOffsetCountSoA.Resize(MeshBodyVertexTriangleMapOffsetCountSoA.Length);
			}
		}
	}

	private void SetData()
	{
		foreach (Body addedBody in m_AddedBodies)
		{
			if (!m_BodyDescriptorsMap.ContainsKey(addedBody))
			{
				Debug.LogError(addedBody.Name);
			}
			int num = m_BodyDescriptorsMap[addedBody];
			BodyDescriptor bodyDesc = BodyDescriptorsSoA[num];
			for (int i = 0; i < bodyDesc.ParticlesOffsetCount.y; i++)
			{
				ParticlesSoA[bodyDesc.ParticlesOffsetCount.x + i] = addedBody.Particles[i];
			}
			for (int j = 0; j < bodyDesc.ConstraintsOffsetCount.y; j++)
			{
				Constraint constraint = addedBody.Constraints[j];
				constraint.SetIndicesOffset(bodyDesc.ParticlesOffsetCount.x);
				ValidateConstraint(ref constraint);
				int index = bodyDesc.ConstraintsOffsetCount.x + j;
				ConstraintsSoA[index] = constraint;
			}
			for (int k = 0; k < bodyDesc.ConstraintsGroupsOffsetCount.y; k++)
			{
				DisconnectedConstraintsGroupsSoA[bodyDesc.ConstraintsGroupsOffsetCount.x + k] = addedBody.DisconnectedConstraintsOffsetCount[k];
			}
			SkinnedBody skinnedBody = addedBody as SkinnedBody;
			if (skinnedBody != null)
			{
				for (int l = 0; l < bodyDesc.SkinnedDataCount; l++)
				{
					SkinnedDataSoA.Bindposes[bodyDesc.SkinnedDataOffset + l] = skinnedBody.Bindposes[l];
					SkinnedDataSoA.Boneposes[bodyDesc.SkinnedDataOffset + l] = skinnedBody.Boneposes[l];
					SkinnedDataSoA.ParentMap[bodyDesc.SkinnedDataOffset + l] = skinnedBody.ParentMap[l];
				}
				for (int m = 0; m < bodyDesc.SkinnedBoneIndicesMapCount; m++)
				{
					SkinnedBodyBoneIndicesMapSoA[bodyDesc.SkinnedBoneIndicesMapOffset + m] = skinnedBody.BoneIndicesMap[m];
				}
			}
			if (addedBody is MeshBody meshBody)
			{
				for (int n = 0; n < bodyDesc.IndicesCount; n++)
				{
					MeshBodyIndicesSoA[bodyDesc.IndicesOffset + n] = meshBody.Indices[n];
				}
				for (int num2 = 0; num2 < bodyDesc.VerticesCount; num2++)
				{
					MeshBodyVerticesSoA.BaseVertices[bodyDesc.VerticesOffset + num2] = meshBody.Vertices[num2];
					MeshBodyVerticesSoA.Vertices[bodyDesc.VerticesOffset + num2] = meshBody.Vertices[num2];
					MeshBodyVerticesSoA.Normals[bodyDesc.VerticesOffset + num2] = meshBody.Normals[num2];
					MeshBodyVerticesSoA.Tangents[bodyDesc.VerticesOffset + num2] = meshBody.Tangents[num2];
					MeshBodyVerticesSoA.Uvs[bodyDesc.VerticesOffset + num2] = meshBody.BaseUvs[num2];
				}
				for (int num3 = 0; num3 < bodyDesc.VertexTriangleMapCount; num3++)
				{
					MeshBodyVertexTriangleMapSoA[bodyDesc.VertexTriangleMapOffset + num3] = meshBody.VertexTriangleMap[num3];
				}
				for (int num4 = 0; num4 < bodyDesc.VertexTriangleMapOffsetCountCount; num4++)
				{
					MeshBodyVertexTriangleMapOffsetCountSoA[bodyDesc.VertexTriangleMapOffsetCountOffset + num4] = meshBody.VertexTriangleMapOffsetCount[num4];
				}
			}
			if (m_Gpu && !m_IsReset)
			{
				bool isSkinned = skinnedBody != null;
				SetGpuBodyData(num, ref bodyDesc, isSkinned);
			}
		}
		if (m_Gpu && !m_IsReset)
		{
			foreach (Body dirtyDataBody in m_DirtyDataBodies)
			{
				bool isSkinned2 = dirtyDataBody is SkinnedBody;
				int num5 = m_BodyDescriptorsMap[dirtyDataBody];
				BodyDescriptor bodyDesc2 = BodyDescriptorsSoA[num5];
				SetGpuBodyData(num5, ref bodyDesc2, isSkinned2);
			}
		}
		if (m_BodyDescriptorsMap.Count > BodyDescriptorsIndicesSoA.Length)
		{
			BodyDescriptorsIndicesSoA.Resize((int)((float)m_BodyDescriptorsMap.Count * 1.5f));
		}
		if (m_SkinnedBodyDescriptorsMap.Count > SkinnedBodyDescriptorsIndicesSoA.Length)
		{
			SkinnedBodyDescriptorsIndicesSoA.Resize((int)((float)m_SkinnedBodyDescriptorsMap.Count * 1.5f));
		}
		if (m_MeshBodyDescriptorsMap.Count > MeshBodyDescriptorsIndicesSoA.Length)
		{
			MeshBodyDescriptorsIndicesSoA.Resize((int)((float)m_MeshBodyDescriptorsMap.Count * 1.5f));
		}
		RebuildBodyGroups();
		int num6 = 0;
		foreach (Body item in m_BodyGroups[BodyGroups.GroupGrass])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item];
			num6++;
		}
		foreach (Body item2 in m_BodyGroups[BodyGroups.Group32])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item2];
			num6++;
		}
		foreach (Body item3 in m_BodyGroups[BodyGroups.Group64])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item3];
			num6++;
		}
		foreach (Body item4 in m_BodyGroups[BodyGroups.Group128])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item4];
			num6++;
		}
		foreach (Body item5 in m_BodyGroups[BodyGroups.Group192])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item5];
			num6++;
		}
		foreach (Body item6 in m_BodyGroups[BodyGroups.Group256])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item6];
			num6++;
		}
		foreach (Body item7 in m_BodyGroups[BodyGroups.Group512])
		{
			BodyDescriptorsIndicesSoA[num6] = m_BodyDescriptorsMap[item7];
			num6++;
		}
		num6 = 0;
		foreach (KeyValuePair<SkinnedBody, int> item8 in m_SkinnedBodyDescriptorsMap)
		{
			SkinnedBodyDescriptorsIndicesSoA[num6] = item8.Value;
			num6++;
		}
		num6 = 0;
		foreach (KeyValuePair<MeshBody, int> item9 in m_MeshBodyDescriptorsMap)
		{
			MeshBodyDescriptorsIndicesSoA[num6] = item9.Value;
			num6++;
		}
		if (m_Gpu)
		{
			GpuData.ParticlesCount = ParticlesCount;
			GpuData.ConstraintsCount = ConstraintsCount;
			GpuData.GrassParticlesCount = m_BodyGroups[BodyGroups.GroupGrass].Sum((Body b) => b.Particles.Count);
			GpuData.BodyDescriptorsMap = BodyDescriptorsMap;
			GpuData.CPUBodyDescriptors = BodyDescriptorsSoA;
			GpuData.BodyGroupsOffsetCount = m_BodyGroupsOffsetCount;
			GpuData.SkinnedBodyDescriptorsMap = m_SkinnedBodyDescriptorsMap;
			GpuData.MeshBodyDescriptorsMap = m_MeshBodyDescriptorsMap;
			if (GpuData.BodyGroupsIndicesSoA.Length != BodyDescriptorsIndicesSoA.Length)
			{
				GpuData.BodyGroupsIndicesSoA.Resize(BodyDescriptorsIndicesSoA.Length);
			}
			if (GpuData.SkinnedBodyDescriptorsIndicesSoA.Length != SkinnedBodyDescriptorsIndicesSoA.Length)
			{
				GpuData.SkinnedBodyDescriptorsIndicesSoA.Resize(SkinnedBodyDescriptorsIndicesSoA.Length);
			}
			if (GpuData.MeshBodyDescriptorsIndicesSoA.Length != MeshBodyDescriptorsIndicesSoA.Length)
			{
				GpuData.MeshBodyDescriptorsIndicesSoA.Resize(MeshBodyDescriptorsIndicesSoA.Length);
			}
			if (GpuData.BodyWorldToLocalMatricesSoA.Length != BodyDescriptorsSoA.Length)
			{
				GpuData.BodyWorldToLocalMatricesSoA.Resize(BodyDescriptorsSoA.Length);
			}
			GpuData.BodyGroupsIndicesSoA.Buffer.SetData(BodyDescriptorsIndicesSoA.Array);
			GpuData.SkinnedBodyDescriptorsIndicesSoA.Buffer.SetData(SkinnedBodyDescriptorsIndicesSoA.Array);
			GpuData.MeshBodyDescriptorsIndicesSoA.Buffer.SetData(MeshBodyDescriptorsIndicesSoA.Array);
			if (m_IsReset)
			{
				CopyAllBodiesDataOnGpu();
			}
			GpuData.IsValid = true;
		}
	}

	private void RebuildBodyGroups()
	{
		foreach (BodyGroups key2 in m_BodyGroups.Keys)
		{
			m_BodyGroups[key2].Clear();
			m_BodyGroupsOffsetCount[key2] = default(int2);
		}
		foreach (KeyValuePair<Body, int> item in m_BodyDescriptorsMap)
		{
			Body key = item.Key;
			if (key is GrassBody)
			{
				m_BodyGroups[BodyGroups.GroupGrass].Add(key);
				continue;
			}
			if (key.Particles.Count <= 32)
			{
				m_BodyGroups[BodyGroups.Group32].Add(key);
				continue;
			}
			if (key.Particles.Count <= 64)
			{
				m_BodyGroups[BodyGroups.Group64].Add(key);
				continue;
			}
			if (key.Particles.Count <= 128)
			{
				m_BodyGroups[BodyGroups.Group128].Add(key);
				continue;
			}
			if (key.Particles.Count <= 192)
			{
				m_BodyGroups[BodyGroups.Group192].Add(key);
				continue;
			}
			if (key.Particles.Count <= 256)
			{
				m_BodyGroups[BodyGroups.Group256].Add(key);
				continue;
			}
			if (key.Particles.Count <= 512)
			{
				m_BodyGroups[BodyGroups.Group512].Add(key);
				continue;
			}
			throw new NotImplementedException("Bodies with particles count more than 512 is not supported.");
		}
		int num = 0;
		m_BodyGroupsOffsetCount[BodyGroups.GroupGrass] = new int2(num, m_BodyGroups[BodyGroups.GroupGrass].Count);
		num += m_BodyGroups[BodyGroups.GroupGrass].Count;
		m_BodyGroupsOffsetCount[BodyGroups.Group32] = new int2(num, m_BodyGroups[BodyGroups.Group32].Count);
		num += m_BodyGroups[BodyGroups.Group32].Count;
		m_BodyGroupsOffsetCount[BodyGroups.Group64] = new int2(num, m_BodyGroups[BodyGroups.Group64].Count);
		num += m_BodyGroups[BodyGroups.Group64].Count;
		m_BodyGroupsOffsetCount[BodyGroups.Group128] = new int2(num, m_BodyGroups[BodyGroups.Group128].Count);
		num += m_BodyGroups[BodyGroups.Group128].Count;
		m_BodyGroupsOffsetCount[BodyGroups.Group192] = new int2(num, m_BodyGroups[BodyGroups.Group192].Count);
		num += m_BodyGroups[BodyGroups.Group192].Count;
		m_BodyGroupsOffsetCount[BodyGroups.Group256] = new int2(num, m_BodyGroups[BodyGroups.Group256].Count);
		num += m_BodyGroups[BodyGroups.Group256].Count;
		m_BodyGroupsOffsetCount[BodyGroups.Group512] = new int2(num, m_BodyGroups[BodyGroups.Group512].Count);
		num += m_BodyGroups[BodyGroups.Group512].Count;
	}

	private void SetGpuBodyData(int bodyDescriptorIndex, ref BodyDescriptor bodyDesc, bool isSkinned)
	{
		GpuData.BodyDescriptorsSoA.ParticlesOffsetCountBuffer.SetData(BodyDescriptorsSoA.ParticlesOffsetCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.ConstraintsOffsetCountBuffer.SetData(BodyDescriptorsSoA.ConstraintsOffsetCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.ConstraintsGroupsOffsetCountBuffer.SetData(BodyDescriptorsSoA.ConstraintsGroupsOffsetCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.SkinnedDataOffsetBuffer.SetData(BodyDescriptorsSoA.SkinnedDataOffset, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.SkinnedDataCountBuffer.SetData(BodyDescriptorsSoA.SkinnedDataCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.IndicesOffsetBuffer.SetData(BodyDescriptorsSoA.IndicesOffset, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.IndicesCountBuffer.SetData(BodyDescriptorsSoA.IndicesCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.VerticesOffsetBuffer.SetData(BodyDescriptorsSoA.VerticesOffset, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.VerticesCountBuffer.SetData(BodyDescriptorsSoA.VerticesCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapOffsetBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapOffset, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapCountBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapOffsetCountOffsetBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapOffsetCountOffset, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapOffsetCountCountBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapOffsetCountCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.LocalCollidersOffsetCountBuffer.SetData(BodyDescriptorsSoA.LocalCollidersOffsetCount, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.MaterialParametersBuffer.SetData(BodyDescriptorsSoA.MaterialParameters, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.BodyDescriptorsSoA.TeleportDistanceTresholdBuffer.SetData(BodyDescriptorsSoA.TeleportDistanceTreshold, bodyDescriptorIndex, bodyDescriptorIndex, 1);
		GpuData.ParticlesSoA.PositionPairsBuffer.SetData(ParticlesSoA.PositionPairs, bodyDesc.ParticlesOffsetCount.x, bodyDesc.ParticlesOffsetCount.x, bodyDesc.ParticlesOffsetCount.y);
		GpuData.ParticlesSoA.MotionPairsBuffer.SetData(ParticlesSoA.MotionPairs, bodyDesc.ParticlesOffsetCount.x, bodyDesc.ParticlesOffsetCount.x, bodyDesc.ParticlesOffsetCount.y);
		GpuData.ParticlesSoA.ExtendedDataBuffer.SetData(ParticlesSoA.ExtendedData, bodyDesc.ParticlesOffsetCount.x, bodyDesc.ParticlesOffsetCount.x, bodyDesc.ParticlesOffsetCount.y);
		GpuData.ConstraintsSoA.DataBuffer.SetData(ConstraintsSoA.m_ConstraintData, bodyDesc.ConstraintsOffsetCount.x, bodyDesc.ConstraintsOffsetCount.x, bodyDesc.ConstraintsOffsetCount.y);
		GpuData.DisconnectedConstraintsGroupsSoA.Buffer.SetData(DisconnectedConstraintsGroupsSoA.Array, bodyDesc.ConstraintsGroupsOffsetCount.x, bodyDesc.ConstraintsGroupsOffsetCount.x, bodyDesc.ConstraintsGroupsOffsetCount.y);
		if (isSkinned)
		{
			GpuData.SkinnedBodySoA.BindposesBuffer.SetData(SkinnedDataSoA.Bindposes, bodyDesc.SkinnedDataOffset, bodyDesc.SkinnedDataOffset, bodyDesc.SkinnedDataCount);
			GpuData.SkinnedBodySoA.BoneposesBuffer.SetData(SkinnedDataSoA.Boneposes, bodyDesc.SkinnedDataOffset, bodyDesc.SkinnedDataOffset, bodyDesc.SkinnedDataCount);
			GpuData.SkinnedBodySoA.ParentMapBuffer.SetData(SkinnedDataSoA.ParentMap, bodyDesc.SkinnedDataOffset, bodyDesc.SkinnedDataOffset, bodyDesc.SkinnedDataCount);
			GpuData.SkinnedBodyBoneIndicesMapSoA.Buffer.SetData(SkinnedBodyBoneIndicesMapSoA.Array, bodyDesc.SkinnedBoneIndicesMapOffset, bodyDesc.SkinnedBoneIndicesMapOffset, bodyDesc.SkinnedBoneIndicesMapCount);
		}
		else
		{
			GpuData.MeshBodyVerticesSoA.VerticesBuffer.SetData(MeshBodyVerticesSoA.BaseVertices, bodyDesc.VerticesOffset, bodyDesc.VerticesOffset, bodyDesc.VerticesCount);
			GpuData.MeshBodyVerticesSoA.NormalsBuffer.SetData(MeshBodyVerticesSoA.Normals, bodyDesc.VerticesOffset, bodyDesc.VerticesOffset, bodyDesc.VerticesCount);
			GpuData.MeshBodyVerticesSoA.UvsBuffer.SetData(MeshBodyVerticesSoA.Uvs, bodyDesc.VerticesOffset, bodyDesc.VerticesOffset, bodyDesc.VerticesCount);
			GpuData.MeshBodyVerticesSoA.TangentsBuffer.SetData(MeshBodyVerticesSoA.Tangents, bodyDesc.VerticesOffset, bodyDesc.VerticesOffset, bodyDesc.VerticesCount);
			GpuData.MeshBodyIndicesSoA.Buffer.SetData(MeshBodyIndicesSoA.Array, bodyDesc.IndicesOffset, bodyDesc.IndicesOffset, bodyDesc.IndicesCount);
			GpuData.MeshBodyVertexTriangleMapSoA.Buffer.SetData(MeshBodyVertexTriangleMapSoA.Array, bodyDesc.VertexTriangleMapOffset, bodyDesc.VertexTriangleMapOffset, bodyDesc.VertexTriangleMapCount);
			GpuData.MeshBodyVertexTriangleMapOffsetCountSoA.Buffer.SetData(MeshBodyVertexTriangleMapOffsetCountSoA.Array, bodyDesc.VertexTriangleMapOffsetCountOffset, bodyDesc.VertexTriangleMapOffsetCountOffset, bodyDesc.VertexTriangleMapOffsetCountCount);
		}
		GpuData.Version++;
	}

	private void CopyAllBodiesDataOnGpu()
	{
		GpuData.BodyDescriptorsSoA.ParticlesOffsetCountBuffer.SetData(BodyDescriptorsSoA.ParticlesOffsetCount);
		GpuData.BodyDescriptorsSoA.ConstraintsOffsetCountBuffer.SetData(BodyDescriptorsSoA.ConstraintsOffsetCount);
		GpuData.BodyDescriptorsSoA.ConstraintsGroupsOffsetCountBuffer.SetData(BodyDescriptorsSoA.ConstraintsGroupsOffsetCount);
		GpuData.BodyDescriptorsSoA.SkinnedDataOffsetBuffer.SetData(BodyDescriptorsSoA.SkinnedDataOffset);
		GpuData.BodyDescriptorsSoA.SkinnedDataCountBuffer.SetData(BodyDescriptorsSoA.SkinnedDataCount);
		GpuData.BodyDescriptorsSoA.IndicesOffsetBuffer.SetData(BodyDescriptorsSoA.IndicesOffset);
		GpuData.BodyDescriptorsSoA.IndicesCountBuffer.SetData(BodyDescriptorsSoA.IndicesCount);
		GpuData.BodyDescriptorsSoA.VerticesOffsetBuffer.SetData(BodyDescriptorsSoA.VerticesOffset);
		GpuData.BodyDescriptorsSoA.VerticesCountBuffer.SetData(BodyDescriptorsSoA.VerticesCount);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapOffsetBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapOffset);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapCountBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapCount);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapOffsetCountOffsetBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapOffsetCountOffset);
		GpuData.BodyDescriptorsSoA.VertexTriangleMapOffsetCountCountBuffer.SetData(BodyDescriptorsSoA.VertexTriangleMapOffsetCountCount);
		GpuData.BodyDescriptorsSoA.LocalCollidersOffsetCountBuffer.SetData(BodyDescriptorsSoA.LocalCollidersOffsetCount);
		GpuData.BodyDescriptorsSoA.MaterialParametersBuffer.SetData(BodyDescriptorsSoA.MaterialParameters);
		GpuData.BodyDescriptorsSoA.TeleportDistanceTresholdBuffer.SetData(BodyDescriptorsSoA.TeleportDistanceTreshold);
		GpuData.ParticlesSoA.PositionPairsBuffer.SetData(ParticlesSoA.PositionPairs, 0, 0, ParticlesCount);
		GpuData.ParticlesSoA.MotionPairsBuffer.SetData(ParticlesSoA.MotionPairs, 0, 0, ParticlesCount);
		GpuData.ParticlesSoA.ExtendedDataBuffer.SetData(ParticlesSoA.ExtendedData, 0, 0, ParticlesCount);
		GpuData.ConstraintsSoA.DataBuffer.SetData(ConstraintsSoA.m_ConstraintData, 0, 0, ConstraintsCount);
		GpuData.DisconnectedConstraintsGroupsSoA.Buffer.SetData(DisconnectedConstraintsGroupsSoA.Array);
		GpuData.SkinnedBodySoA.BindposesBuffer.SetData(SkinnedDataSoA.Bindposes);
		GpuData.SkinnedBodySoA.BoneposesBuffer.SetData(SkinnedDataSoA.Boneposes);
		GpuData.SkinnedBodySoA.ParentMapBuffer.SetData(SkinnedDataSoA.ParentMap);
		GpuData.SkinnedBodyBoneIndicesMapSoA.Buffer.SetData(SkinnedBodyBoneIndicesMapSoA.Array);
		GpuData.MeshBodyVerticesSoA.VerticesBuffer.SetData(MeshBodyVerticesSoA.BaseVertices);
		GpuData.MeshBodyVerticesSoA.NormalsBuffer.SetData(MeshBodyVerticesSoA.Normals);
		GpuData.MeshBodyVerticesSoA.UvsBuffer.SetData(MeshBodyVerticesSoA.Uvs);
		GpuData.MeshBodyVerticesSoA.TangentsBuffer.SetData(MeshBodyVerticesSoA.Tangents);
		GpuData.MeshBodyIndicesSoA.Buffer.SetData(MeshBodyIndicesSoA.Array);
		GpuData.MeshBodyVertexTriangleMapSoA.Buffer.SetData(MeshBodyVertexTriangleMapSoA.Array);
		GpuData.MeshBodyVertexTriangleMapOffsetCountSoA.Buffer.SetData(MeshBodyVertexTriangleMapOffsetCountSoA.Array);
		GpuData.Version++;
	}

	private void ValidateConstraint(ref Constraint constraint)
	{
		constraint.parameters0 = math.saturate(constraint.parameters0);
	}

	internal void BuildColliders()
	{
		CollidersRefs.Sort(SortColliders);
		GlobalCollidersCount = CollidersRefs.Count((ColliderRef c) => c.IsGlobal);
		if (m_Gpu)
		{
			GpuData.SetColliders(CollidersRefs, GlobalCollidersCount);
		}
	}

	private int SortColliders(ColliderRef x, ColliderRef y)
	{
		if (x.IsGlobal || y.IsGlobal)
		{
			int num = (x.IsGlobal ? 1 : 0);
			return (y.IsGlobal ? 1 : 0) - num;
		}
		return x.Owner.GetHashCode() - y.Owner.GetHashCode();
	}

	internal void BuildBodyColliderDependencies()
	{
		IEnumerable<IGrouping<Body, ColliderRef>> enumerable = from cr in CollidersRefs.Skip(GlobalCollidersCount)
			group cr by cr.Owner;
		int num = GlobalCollidersCount;
		foreach (IGrouping<Body, ColliderRef> item in enumerable)
		{
			int num2 = item.Count();
			int bodyDescriptorIndex = GetBodyDescriptorIndex(item.Key);
			if (bodyDescriptorIndex > -1)
			{
				BodyDescriptorsSoA.LocalCollidersOffsetCount[bodyDescriptorIndex] = new int2(num, num2);
				num += num2;
			}
		}
		if (m_Gpu)
		{
			GpuData.SetBodyCollidersDependencies(BodyDescriptorsSoA);
		}
	}

	private void BuildForceVolumes()
	{
		if (m_Gpu)
		{
			GpuData.SetForceVolumes(ForceVolumes);
		}
	}

	internal int GetBodyDescriptorIndex(Body body)
	{
		if (m_BodyDescriptorsMap.TryGetValue(body, out var value))
		{
			return value;
		}
		return -1;
	}

	internal void RefreshBodyParameters(Body body)
	{
		if (Application.isPlaying && m_BodyDescriptorsMap.TryGetValue(body, out var value))
		{
			BodyDescriptorsSoA.MaterialParameters[value] = new float2(body.Friction, body.Restitution);
		}
	}

	internal void GetParticles(Body body, out ParticleSoASlice particles)
	{
		if (!m_BodyDescriptorsMap.TryGetValue(body, out var value))
		{
			particles = default(ParticleSoASlice);
		}
		else
		{
			particles = ParticlesSoA.GetSlice(BodyDescriptorsSoA.ParticlesOffsetCount[value].x, BodyDescriptorsSoA.ParticlesOffsetCount[value].y);
		}
	}

	internal int GetParticlesOffset(Body body)
	{
		if (!Application.isPlaying)
		{
			return -1;
		}
		if (m_IsBodiesDirty)
		{
			BuildBodies();
		}
		if (m_BodyDescriptorsMap != null && m_BodyDescriptorsMap.TryGetValue(body, out var value))
		{
			return BodyDescriptorsSoA.ParticlesOffsetCount[value].x;
		}
		return -1;
	}

	internal Constraint GetConstraint(int constraintId)
	{
		return default(Constraint);
	}

	internal void SetConstraint(ref Constraint constraint)
	{
	}

	internal int GetSizeInBytes()
	{
		return 0 + (BodyDescriptorsSoA?.GetSizeInBytes() ?? 0) + (BodyDescriptorsIndicesSoA?.GetSizeInBytes() ?? 0) + (MeshBodyDescriptorsIndicesSoA?.GetSizeInBytes() ?? 0) + (SkinnedBodyDescriptorsIndicesSoA?.GetSizeInBytes() ?? 0) + (ParticlesSoA?.GetSizeInBytes() ?? 0) + (ConstraintsSoA?.GetSizeInBytes() ?? 0) + (DisconnectedConstraintsGroupsSoA?.GetSizeInBytes() ?? 0) + (SkinnedDataSoA?.GetSizeInBytes() ?? 0) + (SkinnedBodyBoneIndicesMapSoA?.GetSizeInBytes() ?? 0) + (MeshBodyIndicesSoA?.GetSizeInBytes() ?? 0) + (MeshBodyVerticesSoA?.GetSizeInBytes() ?? 0) + (MeshBodyVertexTriangleMapSoA?.GetSizeInBytes() ?? 0) + (MeshBodyVertexTriangleMapOffsetCountSoA?.GetSizeInBytes() ?? 0) + (Colliders?.GetSizeInBytes() ?? 0) + (ColliderUpdateBuffers?.GetSizeInBytes() ?? 0) + (BodyUpdateBuffersSoA?.GetSizeInBytes() ?? 0) + (ForceVolumesSoA?.GetSizeInBytes() ?? 0);
	}

	internal void RegisterBody(Body body)
	{
		if (m_Gpu)
		{
			if (!(body is GrassBody) && body.Particles.Count > 512)
			{
				Debug.LogError("PBD GPU Simulation don't support bodies with more than 512 particles. " + body.Name);
				return;
			}
		}
		else if (body is GrassBody)
		{
			return;
		}
		if (!m_BodyDescriptorsMap.ContainsKey(body))
		{
			m_AddedBodies.Add(body);
			m_IsBodiesDirty = true;
		}
		m_RemovedBodies.Remove(body);
	}

	internal void RegisterForce(IForce force)
	{
		if (!Forces.Contains(force))
		{
			Forces.Add(force);
			if (m_Gpu)
			{
				GpuData.SetForces(Forces);
			}
		}
	}

	internal void RegisterCollider(ColliderRef colliderRef)
	{
		if (!CollidersRefs.Contains(colliderRef))
		{
			CollidersRefs.Add(colliderRef);
			m_IsCollidersDirty = true;
		}
	}

	internal void RegisterForceVolume(ForceVolume forceVolume)
	{
		if (!ForceVolumes.Contains(forceVolume))
		{
			ForceVolumes.Add(forceVolume);
			m_IsForceVolumesDirty = true;
		}
	}

	internal void UnregisterBody(Body body)
	{
		if (m_BodyDescriptorsMap.ContainsKey(body))
		{
			m_RemovedBodies.Add(body);
			m_DirtyDataBodies.Remove(body);
			m_IsBodiesDirty = true;
		}
		else
		{
			m_AddedBodies.Remove(body);
		}
	}

	internal void UpdateDirtyBodyData(Body body)
	{
		if (m_BodyDescriptorsMap.ContainsKey(body))
		{
			m_DirtyDataBodies.Add(body);
			m_IsBodiesDirty = true;
		}
	}

	internal void UnregisterForce(IForce force)
	{
		if (Forces.Remove(force) && m_Gpu)
		{
			GpuData.SetForces(Forces);
		}
	}

	internal void UnregisterCollider(ColliderRef colliderRef)
	{
		if (CollidersRefs.Remove(colliderRef))
		{
			m_IsCollidersDirty = true;
		}
	}

	internal void UnregisterLocalColliders(Body body)
	{
		for (int i = 0; i < CollidersRefs.Count; i++)
		{
			if (CollidersRefs[i].Owner == body)
			{
				CollidersRefs.RemoveAt(i);
				i--;
			}
		}
	}

	internal void UnregisterForceVolume(ForceVolume forceVolume)
	{
		if (ForceVolumes.Remove(forceVolume))
		{
			m_IsForceVolumesDirty = true;
		}
	}
}
