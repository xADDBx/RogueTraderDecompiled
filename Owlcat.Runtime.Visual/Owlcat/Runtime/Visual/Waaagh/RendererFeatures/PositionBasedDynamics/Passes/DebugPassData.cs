using Owlcat.Runtime.Core.Physics.PositionBasedDynamics.GPU;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.PositionBasedDynamics.Passes;

public class DebugPassData : PassDataBase
{
	public TextureHandle CameraColorRT;

	public PositionBasedDynamicsFeature Feature;

	public Material DebugMaterial;

	public GPUData GpuData;

	public Camera Camera;

	public uint[] DrawParticlesArgs;

	public uint[] DrawConstraintsArgs;

	public uint[] DrawNormalsArgs;

	public uint[] DrawCollidersGridArgs;

	public uint[] DrawCollidersAabbArgs;

	public uint[] DrawForceVolumesAabbArgs;

	public uint[] DrawForceVolumesGridArgs;

	public uint[] DrawBodiesAabbArgs;
}
