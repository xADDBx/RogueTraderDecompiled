using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

internal class SceneViewDepthCopyPass : ScriptableRenderPass
{
	private Material m_CopyDepthMaterial;

	private const string m_ProfilerTag = "Copy Depth for Scene View";

	private RenderTargetHandle source { get; set; }

	public SceneViewDepthCopyPass(RenderPassEvent evt, Material copyDepthMaterial)
	{
		m_CopyDepthMaterial = copyDepthMaterial;
		base.RenderPassEvent = evt;
	}

	public void Setup(RenderTargetHandle source)
	{
		this.source = source;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_CopyDepthMaterial == null)
		{
			Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_CopyDepthMaterial, GetType().Name);
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get("Copy Depth for Scene View");
		CoreUtils.SetRenderTarget(commandBuffer, BuiltinRenderTextureType.CameraTarget);
		commandBuffer.SetGlobalTexture("_CameraDepthAttachment", source.Identifier());
		commandBuffer.EnableShaderKeyword(ShaderKeywordStrings._DEPTH_NO_MSAA);
		commandBuffer.DisableShaderKeyword(ShaderKeywordStrings._DEPTH_MSAA_2);
		commandBuffer.DisableShaderKeyword(ShaderKeywordStrings._DEPTH_MSAA_4);
		commandBuffer.Blit(source.Identifier(), BuiltinRenderTextureType.CameraTarget, m_CopyDepthMaterial);
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
