using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class DrawRendererListPass<T> : ScriptableRenderPass where T : DrawRendererListPassData, new()
{
	private BaseRenderFunc<T, RenderGraphContext> m_RenderFunc;

	private RendererList m_RendererList;

	protected DrawRendererListPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	public override void ConfigureRendererLists(ref RenderingData renderingData, RenderGraphResources resources)
	{
		GetOrCreateRendererList(ref renderingData, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists, out m_RendererList);
		DependsOn(in m_RendererList);
	}

	protected sealed override void RecordRenderGraph(ref RenderingData renderingData)
	{
		T passData;
		using RenderGraphBuilder builder = renderingData.RenderGraph.AddRenderPass<T>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\Passes\\DrawRendererListPass.cs", 35);
		passData.Resources = renderingData.CameraData.Renderer.RenderGraphResources;
		passData.RendererList = m_RendererList;
		builder.AllowRendererListCulling(value: true);
		Setup(builder, passData, ref renderingData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererList rendererList);

	protected abstract void Setup(RenderGraphBuilder builder, T data, ref RenderingData renderingData);

	protected abstract void Render(T data, RenderGraphContext context);
}
