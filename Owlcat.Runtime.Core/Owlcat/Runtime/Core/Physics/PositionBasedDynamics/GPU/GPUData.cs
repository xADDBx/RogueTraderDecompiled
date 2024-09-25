using System.Collections.Generic;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Bodies;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Collisions;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.Forces;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.MemoryManagement;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;

public class GPUData
{
	public int ParticlesCount;

	public int GrassParticlesCount;

	public GPUParticleSoA ParticlesSoA = new GPUParticleSoA();

	public int ConstraintsCount;

	public GPUConstraintSoA ConstraintsSoA = new GPUConstraintSoA();

	public GPUSingleBufferSoA<int2> DisconnectedConstraintsGroupsSoA = new GPUSingleBufferSoA<int2>("_PbdConstraintsGroupsBuffer");

	public Dictionary<Body, int> BodyDescriptorsMap = new Dictionary<Body, int>();

	public BodyDescriptorSoA CPUBodyDescriptors;

	public Dictionary<BodyGroups, int2> BodyGroupsOffsetCount;

	public GPUSingleBufferSoA<int> BodyGroupsIndicesSoA = new GPUSingleBufferSoA<int>("_PbdBodyGroupsIndicesBuffer");

	public GPUBodyDescriptorSoA BodyDescriptorsSoA = new GPUBodyDescriptorSoA();

	public GPUSkinnedBodySoA SkinnedBodySoA = new GPUSkinnedBodySoA();

	public GPUSingleBufferSoA<int> SkinnedBodyBoneIndicesMapSoA = new GPUSingleBufferSoA<int>("_PbdSkinnedBodyBoneIndicesMap");

	public GPUSingleBufferSoA<int> SkinnedBodyDescriptorsIndicesSoA = new GPUSingleBufferSoA<int>("_PbdSkinnedBodyDescriptorsIndicesBuffer");

	public Dictionary<SkinnedBody, int> SkinnedBodyDescriptorsMap;

	public GPUSingleBufferSoA<float4x4> BodyWorldToLocalMatricesSoA = new GPUSingleBufferSoA<float4x4>("_PbdBodyWorldToLocalMatrices");

	public GPUMeshBodyVerticesSoA MeshBodyVerticesSoA = new GPUMeshBodyVerticesSoA();

	public GPUSingleBufferSoA<int> MeshBodyIndicesSoA = new GPUSingleBufferSoA<int>("_Indices");

	public GPUSingleBufferSoA<int> MeshBodyVertexTriangleMapSoA = new GPUSingleBufferSoA<int>("_VertexTriangleMap");

	public GPUSingleBufferSoA<int> MeshBodyVertexTriangleMapOffsetCountSoA = new GPUSingleBufferSoA<int>("_VertexTriangleMapOffsetCount");

	public GPUSingleBufferSoA<int> MeshBodyDescriptorsIndicesSoA = new GPUSingleBufferSoA<int>("_PbdMeshBodyDescriptorsIndicesBuffer");

	public Dictionary<MeshBody, int> MeshBodyDescriptorsMap;

	public GPUSingleBufferSoA<uint> BodyVisibilitySoA = new GPUSingleBufferSoA<uint>("_PbdBodyVisibilityBuffer");

	private List<IForce> m_Forces = new List<IForce>();

	private List<ForceVolume> m_ForceVolumes;

	private GPUForceVolumeSoA m_ForceVolumesSoA = new GPUForceVolumeSoA();

	private List<ColliderRef> m_ColliderRefs;

	private GPUColliderSoA m_CollidersSoA = new GPUColliderSoA();

	public List<IForce> Forces => m_Forces;

	public HashSet<string> ForceShaderKernels { get; private set; } = new HashSet<string>();


	public GPUColliderSoA CollidersSoA => m_CollidersSoA;

	public List<ColliderRef> ColliderRefs => m_ColliderRefs;

	public int GlobalCollidersCount { get; private set; }

	public List<ForceVolume> ForceVolumes => m_ForceVolumes;

	public GPUForceVolumeSoA ForceVolumesSoA => m_ForceVolumesSoA;

	public int BodiesCount => BodyDescriptorsMap.Count;

	public int BodiesCountWithoutGrass
	{
		get
		{
			if (BodyGroupsOffsetCount == null)
			{
				return 0;
			}
			int num = 0;
			foreach (KeyValuePair<BodyGroups, int2> item in BodyGroupsOffsetCount)
			{
				if (item.Key != 0)
				{
					num += item.Value.y;
				}
			}
			return num;
		}
	}

	public int CollidersCount => m_ColliderRefs.Count;

	public int ForceVolumesCount => ForceVolumes?.Count ?? 0;

	public bool IsValid { get; internal set; }

	public int Version { get; internal set; }

	public void Dispose()
	{
		BodyDescriptorsSoA.Dispose();
		BodyGroupsIndicesSoA.Dispose();
		BodyWorldToLocalMatricesSoA.Dispose();
		ParticlesSoA.Dispose();
		ConstraintsSoA.Dispose();
		DisconnectedConstraintsGroupsSoA.Dispose();
		SkinnedBodyDescriptorsIndicesSoA.Dispose();
		SkinnedBodySoA.Dispose();
		SkinnedBodyBoneIndicesMapSoA.Dispose();
		MeshBodyDescriptorsIndicesSoA.Dispose();
		MeshBodyVerticesSoA.Dispose();
		MeshBodyIndicesSoA.Dispose();
		MeshBodyVertexTriangleMapSoA.Dispose();
		MeshBodyVertexTriangleMapOffsetCountSoA.Dispose();
		m_CollidersSoA.Dispose();
		if (m_ForceVolumesSoA != null)
		{
			m_ForceVolumesSoA.Dispose();
		}
		BodyVisibilitySoA.Dispose();
	}

	internal void SetForces(HashSet<IForce> forces)
	{
		m_Forces.Clear();
		m_Forces.AddRange(forces);
		ForceShaderKernels.Clear();
		foreach (IForce force in m_Forces)
		{
			if (!ForceShaderKernels.Contains(force.ComputeShaderKernel))
			{
				ForceShaderKernels.Add(force.ComputeShaderKernel);
			}
		}
	}

	internal void SetColliders(List<ColliderRef> collidersRefs, int globalCollidersCount)
	{
		m_ColliderRefs = collidersRefs;
		if (m_CollidersSoA == null)
		{
			m_CollidersSoA = new GPUColliderSoA();
		}
		int num = math.max(collidersRefs.Count, 1);
		if (m_CollidersSoA.Length < num)
		{
			m_CollidersSoA.Resize((int)((float)num * 1.5f));
		}
		GlobalCollidersCount = globalCollidersCount;
	}

	internal void SetForceVolumes(List<ForceVolume> forceVolumes)
	{
		m_ForceVolumes = forceVolumes;
		if (m_ForceVolumesSoA == null)
		{
			m_ForceVolumesSoA = new GPUForceVolumeSoA();
		}
		int num = math.max(m_ForceVolumes.Count, 1);
		if (m_ForceVolumesSoA.Length < num)
		{
			m_ForceVolumesSoA.Resize((int)((float)num * 1.5f));
		}
	}

	internal void SetBodyCollidersDependencies(BodyDescriptorSoA bodyDescriptors)
	{
		BodyDescriptorsSoA.LocalCollidersOffsetCountBuffer.SetData(bodyDescriptors.LocalCollidersOffsetCount);
	}
}
