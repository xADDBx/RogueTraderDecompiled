using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

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

	protected override void Setup(RenderGraphBuilder builder, FogPassData data, ref RenderingData renderingData)
	{
		data.Material = m_Material;
		data.CameraColorRT = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		data.CameraDepthCopyRT = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
		builder.DependsOn(in data.Resources.RendererLists.OpaqueGBuffer.List);
		builder.AllowRendererListCulling(value: true);
	}

	protected override void Render(FogPassData data, RenderGraphContext context)
	{
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
