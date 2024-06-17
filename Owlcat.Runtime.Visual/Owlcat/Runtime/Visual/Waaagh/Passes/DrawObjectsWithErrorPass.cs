using System;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public sealed class DrawObjectsWithErrorPass : DrawRendererListPass<DrawObjectsWithErrorPassData>
{
	public enum ErrorType
	{
		UnsupportedMaterials,
		MissingMaterial
	}

	private static readonly ShaderTagId[] s_UnsupportedMaterialPassNames = new ShaderTagId[7]
	{
		new ShaderTagId("Always"),
		new ShaderTagId("ForwardBase"),
		new ShaderTagId("Deferred"),
		new ShaderTagId("PrepassBase"),
		new ShaderTagId("Vertex"),
		new ShaderTagId("VertexLMRGBM"),
		new ShaderTagId("VertexLM")
	};

	private static readonly ShaderTagId[] s_MissingMaterialPassNames = new ShaderTagId[1]
	{
		new ShaderTagId("SRPDefaultUnlit")
	};

	private static readonly RenderQueueRange s_UnsupportedMaterialRenderQueueRange = RenderQueueRange.all;

	private static readonly RenderQueueRange s_MissingMaterialRenderQueueRange = RenderQueueRange.opaque;

	private readonly Material m_ErrorMaterial;

	private readonly ShaderTagId[] m_PassNames;

	private readonly RenderQueueRange m_RenderQueueRange;

	public override string Name => "DrawObjectsWithErrorPass";

	public DrawObjectsWithErrorPass(RenderPassEvent evt, Material errorMaterial, ErrorType errorType)
		: base(evt)
	{
		m_ErrorMaterial = errorMaterial;
		switch (errorType)
		{
		case ErrorType.UnsupportedMaterials:
			m_PassNames = s_UnsupportedMaterialPassNames;
			m_RenderQueueRange = s_UnsupportedMaterialRenderQueueRange;
			break;
		case ErrorType.MissingMaterial:
			m_PassNames = s_MissingMaterialPassNames;
			m_RenderQueueRange = s_MissingMaterialRenderQueueRange;
			break;
		default:
			throw new ArgumentOutOfRangeException("errorType", errorType, null);
		}
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList)
	{
		RenderGraph renderGraph = renderingData.RenderGraph;
		RendererListDesc desc = CreateRendererListDesc(ref renderingData);
		rendererList = renderGraph.CreateRendererList(in desc);
	}

	private RendererListDesc CreateRendererListDesc(ref RenderingData renderingData)
	{
		CullingResults cullingResults = renderingData.CullingResults;
		Camera camera = renderingData.CameraData.Camera;
		ShaderTagId[] passNames = m_PassNames;
		RenderQueueRange? renderQueueRange = m_RenderQueueRange;
		Material errorMaterial = m_ErrorMaterial;
		return RenderingUtils.CreateRendererListDesc(cullingResults, camera, passNames, PerObjectData.None, renderQueueRange, SortingCriteria.CommonOpaque, null, errorMaterial);
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsWithErrorPassData data, ref RenderingData renderingData)
	{
		builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.ReadWrite);
	}

	protected override void Render(DrawObjectsWithErrorPassData data, RenderGraphContext context)
	{
		DrawRendererList(context.cmd, data.RendererList);
	}

	private void DrawRendererList(CommandBuffer cmd, RendererList rendererList)
	{
		if (!rendererList.isValid)
		{
			throw new ArgumentException("Invalid renderer list provided to DrawRendererList");
		}
		cmd.DrawRendererList(rendererList);
	}
}
