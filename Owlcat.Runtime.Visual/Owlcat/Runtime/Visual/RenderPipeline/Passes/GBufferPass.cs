using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class GBufferPass : ScriptableRenderPass
{
	private const string kProfilerTag = "GBuffer Pass";

	private const string kGBufferPassName = "GBUFFER";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("GBuffer Pass");

	private GBuffer m_GBuffer;

	private FilteringSettings m_FilteringSettings;

	private ShaderTagId m_ShaderTagId = new ShaderTagId("GBuffer");

	private RenderStateBlock m_RenderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

	public GBufferPass(RenderPassEvent evt, RenderQueueRange renderQueueRange, LayerMask layerMask)
	{
		base.RenderPassEvent = evt;
		m_FilteringSettings = new FilteringSettings(renderQueueRange, layerMask);
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
			if (m_GBuffer.RenderPath == RenderPath.Forward)
			{
				commandBuffer.SetRenderTarget(m_GBuffer.ForwardGBuffer, m_GBuffer.CameraDepthRt.Identifier());
			}
			else
			{
				commandBuffer.SetRenderTarget(m_GBuffer.DeferredGBuffer, m_GBuffer.CameraDepthRt.Identifier());
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			SortingCriteria defaultOpaqueSortFlags = renderingData.CameraData.DefaultOpaqueSortFlags;
			DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTagId, ref renderingData, defaultOpaqueSortFlags);
			context.DrawRenderers(renderingData.CullResults, ref drawingSettings, ref m_FilteringSettings, ref m_RenderStateBlock);
			commandBuffer.SetGlobalTexture(m_GBuffer.CameraNormalsRt.Id, m_GBuffer.CameraNormalsRt.Identifier());
			commandBuffer.SetGlobalTexture(m_GBuffer.CameraBakedGIRt.Id, m_GBuffer.CameraBakedGIRt.Identifier());
			commandBuffer.SetGlobalTexture(m_GBuffer.CameraShadowmaskRT.Id, m_GBuffer.CameraShadowmaskRT.Identifier());
			if (m_GBuffer.RenderPath == RenderPath.Deferred)
			{
				commandBuffer.SetGlobalTexture(m_GBuffer.CameraAlbedoRt.Id, m_GBuffer.CameraAlbedoRt.Identifier());
				commandBuffer.SetGlobalTexture(m_GBuffer.CameraSpecularRt.Id, m_GBuffer.CameraSpecularRt.Identifier());
				commandBuffer.SetGlobalTexture(m_GBuffer.CameraTranslucencyRT.Id, m_GBuffer.CameraTranslucencyRT.Identifier());
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		IndirectRenderingSystem.Instance.DrawPass(ref context, renderingData.CameraData.Camera.cameraType, renderingData.CameraData.IsIndirectRenderingEnabled, renderingData.CameraData.IsSceneViewInPrefabEditMode, "GBUFFER", m_FilteringSettings.renderQueueRange);
	}
}
