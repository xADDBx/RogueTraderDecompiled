using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DecalPreviewPass : DrawRendererListPass<DecalPreviewPassData>
{
	private ShaderTagId m_ShaderTagId;

	public override string Name => "DecalPreviewPass";

	public DecalPreviewPass(RenderPassEvent evt)
		: base(evt)
	{
		m_ShaderTagId = new ShaderTagId("DecalPreview");
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList)
	{
		RendererListDesc desc = CreateRendererListDesc(ref renderingData);
		rendererList = renderingData.RenderGraph.CreateRendererList(in desc);
	}

	private RendererListDesc CreateRendererListDesc(ref RenderingData renderingData)
	{
		return RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_ShaderTagId, renderingData.PerObjectData, RenderQueueRange.opaque);
	}

	protected override void Setup(RenderGraphBuilder builder, DecalPreviewPassData data, ref RenderingData renderingData)
	{
		data.CameraDepthRT = builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Read);
		data.CameraColorRT = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
	}

	protected override void Render(DecalPreviewPassData data, RenderGraphContext context)
	{
		context.cmd.DrawRendererList(data.RendererList);
	}
}
