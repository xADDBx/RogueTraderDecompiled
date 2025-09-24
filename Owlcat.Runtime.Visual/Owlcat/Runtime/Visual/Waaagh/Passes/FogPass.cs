using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class FogPass : ScriptableRenderPass<FogPassData>
{
	private Material m_Material;

	public override string Name => "FogPass";

	public FogPass(RenderPassEvent evt, Material material)
		: base(evt)
	{
		m_Material = material;
	}

	public override void ConfigureRendererLists(ref RenderingData renderingData, RenderGraphResources resources)
	{
		DependsOn(in resources.RendererLists.OpaqueGBuffer.List);
	}

	protected override void Setup(RenderGraphBuilder builder, FogPassData data, ref RenderingData renderingData)
	{
		data.Material = m_Material;
		data.CameraColorRT = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		data.CameraDepthCopyRT = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
	}

	protected override void Render(FogPassData data, RenderGraphContext context)
	{
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
