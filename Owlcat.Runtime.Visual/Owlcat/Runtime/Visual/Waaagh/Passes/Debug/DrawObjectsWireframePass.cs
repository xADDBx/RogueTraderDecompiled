using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class DrawObjectsWireframePass : DrawRendererListPass<DrawObjectsWireframePassData>
{
	private Color m_ClearColor = new Color(0.5f, 0.5f, 0.5f);

	public override string Name => "DrawObjectsWireframePassData";

	public DrawObjectsWireframePass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList)
	{
		RendererListDesc desc = sharedRendererLists.Transparent.Desc;
		desc.rendererConfiguration = PerObjectData.None;
		desc.renderQueueRange = WaaaghRenderQueue.All;
		rendererList = renderingData.RenderGraph.CreateRendererList(in desc);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsWireframePassData data, ref RenderingData renderingData)
	{
		builder.AllowRendererListCulling(value: false);
		TextureHandle input = data.Resources.FinalTarget;
		data.RenderTarget = builder.UseColorBuffer(in input, 0);
		data.ClearColor = m_ClearColor;
	}

	protected override void Render(DrawObjectsWireframePassData data, RenderGraphContext context)
	{
		context.cmd.ClearRenderTarget(clearDepth: true, clearColor: true, data.ClearColor);
		context.cmd.DrawRendererList(data.RendererList);
	}
}
