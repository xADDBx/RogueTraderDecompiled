using System.Buffers;
using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Broadphase;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Passes;

public class SimulationPassData : PassDataBase
{
	public ComputeShader SimulationShader;

	public int KernelSimulateGrass;

	public int KernelSimulate32;

	public int KernelSimulate64;

	public int KernelSimulate128;

	public int KernelSimulate192;

	public int KernelSimulate256;

	public int KernelSimulate512;

	public ComputeShader CollisionShader;

	public int KernelUpdateColliders;

	public int KernelClearCollidersGrid;

	public int KernelUpdateCollidersGrid;

	public ComputeShader ForceVolumeShader;

	public int KernelUpdateForceVolumes;

	public int KernelClearForceVolumesGrid;

	public int KernelUpdateForceFolumesGrid;

	public ComputeShader SkinningShader;

	public int KernelSkinnedUpdateParticlesSingleDispatch;

	public int KernelSkinnedUpdateBindposesSingleDispatch;

	public ComputeShader MeshShader;

	public int KernelMeshUpdateParticlesSingleDispatch;

	public int KernelMeshUpdateNormalsAndTangentsSingleDispatch;

	public ComputeShader BodyAabbShader;

	public int KernelUpdateBodyAabb32;

	public int KernelUpdateBodyAabb64;

	public int KernelUpdateBodyAabb128;

	public int KernelUpdateBodyAabb192;

	public int KernelUpdateBodyAabb256;

	public int KernelUpdateBodyAabb512;

	public ComputeShader CameraCullingShader;

	public int KernelCameraCull;

	public GPUData GpuData;

	public int SimulationIterations;

	public int ConstraintIterations;

	public float Decay;

	public Camera[] Cameras;

	public GPUBroadphaseBase Broadphase;

	public ArrayPool<Matrix4x4> MatrixPool;

	public ArrayPool<Vector4> Vector4Pool;

	public bool Simulate;

	public bool CameraCullingEnabled;
}
