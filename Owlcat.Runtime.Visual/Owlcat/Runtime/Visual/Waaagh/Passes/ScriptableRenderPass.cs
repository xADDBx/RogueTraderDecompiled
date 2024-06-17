using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class ScriptableRenderPass
{
	public RenderPassEvent RenderPassEvent { get; set; }

	public abstract string Name { get; }

	public ScriptableRenderPass(RenderPassEvent evt)
	{
		RenderPassEvent = evt;
	}

	public void Execute(ref RenderingData renderingData)
	{
		RecordRenderGraph(ref renderingData);
	}

	protected abstract void RecordRenderGraph(ref RenderingData renderingData);

	public static bool operator <(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.RenderPassEvent < rhs.RenderPassEvent;
	}

	public static bool operator >(ScriptableRenderPass lhs, ScriptableRenderPass rhs)
	{
		return lhs.RenderPassEvent > rhs.RenderPassEvent;
	}
}
public abstract class ScriptableRenderPass<T> : ScriptableRenderPass where T : PassDataBase, new()
{
	private RenderFunc<T> m_RenderFunc;

	public ScriptableRenderPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	protected sealed override void RecordRenderGraph(ref RenderingData renderingData)
	{
		T passData;
		using RenderGraphBuilder builder = renderingData.RenderGraph.AddRenderPass<T>(Name, out passData);
		passData.Resources = renderingData.CameraData.Renderer.RenderGraphResources;
		Setup(builder, passData, ref renderingData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void Setup(RenderGraphBuilder builder, T data, ref RenderingData renderingData);

	protected abstract void Render(T data, RenderGraphContext context);
}
