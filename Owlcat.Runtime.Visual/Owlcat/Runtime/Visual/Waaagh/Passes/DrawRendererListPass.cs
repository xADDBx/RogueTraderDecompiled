using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class DrawRendererListPass<T> : ScriptableRenderPass where T : DrawRendererListPassData, new()
{
	private RenderFunc<T> m_RenderFunc;

	protected DrawRendererListPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	protected sealed override void RecordRenderGraph(ref RenderingData renderingData)
	{
		T passData;
		using RenderGraphBuilder builder = renderingData.RenderGraph.AddRenderPass<T>(Name, out passData);
		passData.Resources = renderingData.CameraData.Renderer.RenderGraphResources;
		GetOrCreateRendererList(ref renderingData, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists, out var rendererList);
		passData.RendererList = builder.UseRendererList(in rendererList);
		builder.AllowRendererListCulling(value: true);
		Setup(builder, passData, ref renderingData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList);

	protected abstract void Setup(RenderGraphBuilder builder, T data, ref RenderingData renderingData);

	protected abstract void Render(T data, RenderGraphContext context);
}
