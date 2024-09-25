using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class DistortionPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Draw Distortion Vectors";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Distortion Vectors");

	private ShaderTagId m_ShaderTag;

	private FilteringSettings m_FilteringSettings;

	private RenderTargetHandle m_DistortionVectorsHandle;

	private RenderTargetHandle m_ColorAttachment;

	private RenderTargetHandle m_DepthAttachment;

	private Material m_ApplyDistortionMaterial;

	public DistortionPass(RenderPassEvent evt, Material applyDistortionMaterial)
	{
		base.RenderPassEvent = evt;
		m_ShaderTag = new ShaderTagId("DistortionVectors");
		m_FilteringSettings = new FilteringSettings(OwlcatRenderQueue.Transparent);
		m_ApplyDistortionMaterial = applyDistortionMaterial;
		m_DistortionVectorsHandle.Init("_DistortionVectorsRT");
	}

	public void Setup(RenderTargetHandle colorAttachment, RenderTargetHandle depthAttachment)
	{
		m_ColorAttachment = colorAttachment;
		m_DepthAttachment = depthAttachment;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.CameraData.CameraTargetDescriptor;
			cameraTargetDescriptor.colorFormat = RenderTextureFormat.ARGBHalf;
			cameraTargetDescriptor.depthBufferBits = 0;
			commandBuffer.GetTemporaryRT(m_DistortionVectorsHandle.Id, cameraTargetDescriptor);
			commandBuffer.SetRenderTarget(m_DistortionVectorsHandle.Identifier(), m_DepthAttachment.Identifier());
			commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 0f));
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			DrawingSettings drawingSettings = CreateDrawingSettings(m_ShaderTag, ref renderingData, SortingCriteria.CommonOpaque);
			context.DrawRenderers(renderingData.CullResults, ref drawingSettings, ref m_FilteringSettings);
			commandBuffer.Blit(m_DistortionVectorsHandle.Identifier(), m_ColorAttachment.Identifier(), m_ApplyDistortionMaterial, 0);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			commandBuffer.ReleaseTemporaryRT(m_DistortionVectorsHandle.Id);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
