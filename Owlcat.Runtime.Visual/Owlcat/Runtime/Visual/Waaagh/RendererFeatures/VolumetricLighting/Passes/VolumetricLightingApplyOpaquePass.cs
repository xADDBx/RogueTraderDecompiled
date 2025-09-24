using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class VolumetricLightingApplyOpaquePass : ScriptableRenderPass<VolumetricLightingApplyOpaquePassData>
{
	private Material m_Material;

	private static int _CameraColor = Shader.PropertyToID("_CameraColor");

	public override string Name => "VolumetricLightingApplyOpaquePass";

	public VolumetricLightingApplyOpaquePass(RenderPassEvent evt, Material material)
		: base(evt)
	{
		m_Material = material;
	}

	protected override void Setup(RenderGraphBuilder builder, VolumetricLightingApplyOpaquePassData data, ref RenderingData renderingData)
	{
		data.Material = m_Material;
		data.ScatterTexture = builder.ReadTexture(in data.Resources.VolumetricScatter);
		data.CameraColorBuffer = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		data.DepthCopyTexture = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
	}

	protected override void Render(VolumetricLightingApplyOpaquePassData data, RenderGraphContext context)
	{
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
