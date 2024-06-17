using System.Collections.Generic;
using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

internal class DrawObjectsPass : ScriptableRenderPass
{
	protected const string kForwardLitPassName = "FORWARD LIT";

	private ProfilingSampler m_ProfilingSampler;

	private FilteringSettings m_FilteringSettings;

	private RenderStateBlock m_RenderStateBlock;

	private List<ShaderTagId> m_ShaderTagIdList = new List<ShaderTagId>();

	private string m_ProfilerTag;

	private bool m_IsOpaque;

	private bool m_Clear;

	private GBuffer m_GBuffer;

	public DrawObjectsPass(string profilerTag, bool opaque, bool clear, RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask, StencilState stencilState, int stencilReference)
	{
		m_ProfilerTag = profilerTag;
		m_ProfilingSampler = new ProfilingSampler(profilerTag);
		m_ShaderTagIdList.Add(new ShaderTagId("SRPDefaultUnlit"));
		m_ShaderTagIdList.Add(new ShaderTagId("ForwardLit"));
		base.RenderPassEvent = evt;
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
		m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);
		m_IsOpaque = opaque;
		m_Clear = clear;
		if (stencilState.enabled)
		{
			m_RenderStateBlock.stencilReference = stencilReference;
			m_RenderStateBlock.mask = RenderStateMask.Stencil;
			m_RenderStateBlock.stencilState = stencilState;
		}
	}

	public void Setup(GBuffer gBuffer)
	{
		m_GBuffer = gBuffer;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			commandBuffer.SetRenderTarget(m_GBuffer.CameraColorRt.Identifier(), m_GBuffer.CameraDepthRt.Identifier());
			if (m_IsOpaque && m_Clear && renderingData.CameraData.IsFirstInChain)
			{
				commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, renderingData.CameraData.Camera.backgroundColor);
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			SortingCriteria sortingCriteria = (m_IsOpaque ? renderingData.CameraData.DefaultOpaqueSortFlags : SortingCriteria.CommonTransparent);
			DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagIdList, ref renderingData, sortingCriteria);
			if (renderingData.RenderPath == RenderPath.Forward || m_FilteringSettings.renderQueueRange == OwlcatRenderQueue.OpaqueDistortion || !m_IsOpaque)
			{
				drawingSettings.perObjectData |= PerObjectData.ReflectionProbes;
			}
			context.DrawRenderers(renderingData.CullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		IndirectRenderingSystem.Instance.DrawPass(ref context, renderingData.CameraData.Camera.cameraType, renderingData.CameraData.IsIndirectRenderingEnabled, renderingData.CameraData.IsSceneViewInPrefabEditMode, "FORWARD LIT", m_FilteringSettings.renderQueueRange);
	}
}
