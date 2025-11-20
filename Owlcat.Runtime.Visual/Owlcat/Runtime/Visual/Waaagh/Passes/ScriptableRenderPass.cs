using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public abstract class ScriptableRenderPass
{
	private List<RendererList> m_RendererLists = new List<RendererList>();

	public RenderPassEvent RenderPassEvent { get; set; }

	public abstract string Name { get; }

	internal List<RendererList> UsedRendererLists => m_RendererLists;

	public ScriptableRenderPass(RenderPassEvent evt)
	{
		RenderPassEvent = evt;
	}

	public void DependsOn(in RendererList rendererList)
	{
		m_RendererLists.Add(rendererList);
	}

	public void ClearRendererLists()
	{
		m_RendererLists.Clear();
	}

	public virtual bool AreRendererListsEmpty(ScriptableRenderContext context)
	{
		int count = m_RendererLists.Count;
		for (int i = 0; i < count; i++)
		{
			if (context.QueryRendererListStatus(m_RendererLists[i]) == RendererListStatus.kRendererListPopulated)
			{
				return false;
			}
		}
		if (m_RendererLists.Count > 0)
		{
			return true;
		}
		return false;
	}

	public void Execute(ref RenderingData renderingData)
	{
		RecordRenderGraph(ref renderingData);
	}

	public virtual void ConfigureRendererLists(ref RenderingData renderingData, RenderGraphResources resources)
	{
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
	private BaseRenderFunc<T, RenderGraphContext> m_RenderFunc;

	public ScriptableRenderPass(RenderPassEvent evt)
		: base(evt)
	{
		m_RenderFunc = Render;
	}

	protected sealed override void RecordRenderGraph(ref RenderingData renderingData)
	{
		T passData;
		using RenderGraphBuilder builder = renderingData.RenderGraph.AddRenderPass<T>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@94246ccf1d50\\Runtime\\Waaagh\\Passes\\ScriptableRenderPass.cs", 100);
		passData.Resources = renderingData.CameraData.Renderer.RenderGraphResources;
		Setup(builder, passData, ref renderingData);
		builder.SetRenderFunc(m_RenderFunc);
	}

	protected abstract void Setup(RenderGraphBuilder builder, T data, ref RenderingData renderingData);

	protected abstract void Render(T data, RenderGraphContext context);
}
