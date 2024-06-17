using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

public class ClearPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Clear Render Targets";

	private readonly Color m_ClearColor = new Color(0f, 0f, 0f, 0f);

	private GBuffer m_GBuffer;

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Clear Render Targets");

	public ClearPass(RenderPassEvent evt)
	{
		base.RenderPassEvent = evt;
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
			float depth = (SystemInfo.usesReversedZBuffer ? 1 : 0);
			if (renderingData.CameraData.IsFirstInChain || renderingData.CameraData.Camera.clearFlags == CameraClearFlags.Skybox || renderingData.CameraData.Camera.clearFlags == CameraClearFlags.Color)
			{
				if (m_GBuffer.RenderPath == RenderPath.Forward)
				{
					commandBuffer.SetRenderTarget(m_GBuffer.ForwardGBuffer, m_GBuffer.CameraDepthRt.Identifier());
					commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, m_ClearColor, depth);
				}
				else
				{
					commandBuffer.SetRenderTarget(m_GBuffer.DeferredGBuffer, m_GBuffer.CameraDepthRt.Identifier());
					commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: true, m_ClearColor, depth);
					commandBuffer.SetRenderTarget(m_GBuffer.CameraColorRt.Identifier());
					commandBuffer.ClearRenderTarget(clearDepth: false, clearColor: true, renderingData.CameraData.Camera.backgroundColor.linear);
				}
			}
			else if (renderingData.CameraData.Camera.clearFlags == CameraClearFlags.Depth)
			{
				commandBuffer.SetRenderTarget(m_GBuffer.CameraDepthRt.Identifier());
				commandBuffer.ClearRenderTarget(clearDepth: true, clearColor: false, m_ClearColor, depth);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
