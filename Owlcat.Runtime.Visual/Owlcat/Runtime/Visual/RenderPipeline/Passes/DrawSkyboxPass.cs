using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

internal class DrawSkyboxPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Draw Skybox";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Draw Skybox");

	private RenderTargetHandle m_ColorAttachment;

	private RenderTargetHandle m_DepthAttachment;

	public DrawSkyboxPass(RenderPassEvent evt)
	{
		base.RenderPassEvent = evt;
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
			commandBuffer.SetRenderTarget(m_ColorAttachment.Identifier(), m_DepthAttachment.Identifier());
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
		context.DrawSkybox(renderingData.CameraData.Camera);
	}
}
