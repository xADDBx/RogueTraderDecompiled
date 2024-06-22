using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public sealed class DebugQuadOverdrawPass : ScriptableRenderPass<DebugQuadOverdrawPass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public readonly List<RendererListHandle> RendererLists = new List<RendererListHandle>();

		public bool DepthHelperPlaneEneabled;

		public float DepthHelperPlaneLevel;

		public ComputeBufferHandle FullScreenDebugBuffer;

		public void Clear()
		{
			RendererLists.Clear();
		}
	}

	private readonly WaaaghDebugData m_DebugData;

	private readonly RenderGraphDebugResources m_Resources;

	private readonly Shader m_Shader;

	private readonly int m_OverdrawShaderPassIndex;

	private readonly int m_DepthOnlyPassIndex;

	private readonly Material m_HelperDepthPlaneMaterial;

	public override string Name => "DebugQuadOverdrawPass";

	public DebugQuadOverdrawPass(WaaaghDebugData debugData, RenderGraphDebugResources resources)
		: base(RenderPassEvent.AfterRenderingTransparents)
	{
		m_DebugData = debugData;
		m_Resources = resources;
		m_Shader = m_DebugData.Shaders.DebugOverdrawPS;
		m_OverdrawShaderPassIndex = 0;
		m_DepthOnlyPassIndex = 1;
		m_HelperDepthPlaneMaterial = CoreUtils.CreateEngineMaterial(m_Shader);
	}

	protected override void Setup(RenderGraphBuilder builder, PassData passData, ref RenderingData renderingData)
	{
		if (m_DebugData.RenderingDebug.OverdrawMode != DebugOverdrawMode.QuadOverdraw || !m_Resources.FullScreenDebugBuffer.IsValid() || m_Shader == null)
		{
			return;
		}
		passData.Clear();
		bool flag = m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.All || m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.OpaqueOnly;
		bool flag2 = m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.All || m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.TransparentOnly;
		if (m_DebugData.RenderingDebug.QuadOverdrawSettings.DepthTestMode == QuadOverdrawDepthTestMode.PrePass && flag)
		{
			passData.RendererLists.Add(CreateDepthPrePassRendererList(renderingData.RenderGraph, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.OpaqueGBuffer.Desc));
			passData.RendererLists.Add(CreateDepthPrePassRendererList(renderingData.RenderGraph, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.OpaqueDistortionGBuffer.Desc));
		}
		if (flag)
		{
			DepthState depthState = m_DebugData.RenderingDebug.QuadOverdrawSettings.DepthTestMode switch
			{
				QuadOverdrawDepthTestMode.Disabled => new DepthState(writeEnabled: false, CompareFunction.Always), 
				QuadOverdrawDepthTestMode.Enabled => new DepthState(writeEnabled: true, CompareFunction.LessEqual), 
				QuadOverdrawDepthTestMode.PrePass => new DepthState(writeEnabled: false, CompareFunction.Equal), 
				_ => throw new ArgumentOutOfRangeException(), 
			};
			passData.RendererLists.Add(CreateOverdrawRendererLists(renderingData.RenderGraph, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.OpaqueGBuffer.Desc, depthState));
			passData.RendererLists.Add(CreateOverdrawRendererLists(renderingData.RenderGraph, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.OpaqueDistortionGBuffer.Desc, depthState));
		}
		if (flag2)
		{
			DepthState depthState2 = new DepthState(writeEnabled: false, CompareFunction.LessEqual);
			passData.RendererLists.Add(CreateOverdrawRendererLists(renderingData.RenderGraph, renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.Transparent.Desc, depthState2));
		}
		TextureHandle input = passData.Resources.FinalTarget;
		builder.UseColorBuffer(in input, 0);
		builder.UseDepthBuffer(in passData.Resources.FinalTargetDepth, DepthAccess.ReadWrite);
		passData.FullScreenDebugBuffer = builder.WriteComputeBuffer(in m_Resources.FullScreenDebugBuffer);
		builder.AllowRendererListCulling(value: false);
		foreach (RendererListHandle rendererList in passData.RendererLists)
		{
			RendererListHandle input2 = rendererList;
			builder.UseRendererList(in input2);
		}
		passData.DepthHelperPlaneEneabled = m_DebugData.RenderingDebug.QuadOverdrawSettings.DepthTestMode == QuadOverdrawDepthTestMode.Enabled && m_DebugData.RenderingDebug.QuadOverdrawSettings.DepthHelperPlaneEneabled;
		passData.DepthHelperPlaneLevel = m_DebugData.RenderingDebug.QuadOverdrawSettings.DepthHelperPlaneLevel;
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		context.cmd.SetRandomWriteTarget(1, data.FullScreenDebugBuffer);
		context.cmd.ClearRenderTarget(clearDepth: true, clearColor: true, Color.clear);
		if (data.DepthHelperPlaneEneabled)
		{
			context.cmd.DrawMesh(RenderingUtils.QuadMesh, Matrix4x4.TRS(new Vector3(0f, data.DepthHelperPlaneLevel, 0f), Quaternion.AngleAxis(90f, Vector3.right), new Vector3(10000f, 10000f, 10000f)), m_HelperDepthPlaneMaterial, 0, m_DepthOnlyPassIndex);
		}
		foreach (RendererListHandle rendererList in data.RendererLists)
		{
			context.cmd.DrawRendererList(rendererList);
		}
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
	}

	private RendererListHandle CreateOverdrawRendererLists(RenderGraph renderGraph, RendererListDesc baseDesc, DepthState depthState)
	{
		RendererListDesc desc = baseDesc;
		desc.rendererConfiguration = PerObjectData.None;
		desc.overrideShader = m_Shader;
		desc.overrideShaderPassIndex = m_OverdrawShaderPassIndex;
		desc.stateBlock = new RenderStateBlock(RenderStateMask.Depth)
		{
			depthState = depthState
		};
		return renderGraph.CreateRendererList(in desc);
	}

	private RendererListHandle CreateDepthPrePassRendererList(RenderGraph renderGraph, RendererListDesc baseDesc)
	{
		RendererListDesc desc = baseDesc;
		desc.rendererConfiguration = PerObjectData.None;
		desc.overrideShader = m_Shader;
		desc.overrideShaderPassIndex = m_DepthOnlyPassIndex;
		return renderGraph.CreateRendererList(in desc);
	}
}
