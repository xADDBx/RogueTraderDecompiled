using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.Passes;

internal class FinalBlitPass : ScriptableRenderPass
{
	private const string m_ProfilerTag = "Final Blit Pass";

	private RenderTargetHandle m_Source;

	private RenderTargetHandle m_Dest;

	private Material m_BlitMaterial;

	private RenderTextureDescriptor m_Desc;

	private RenderTargetHandle m_GammaConvertSceneView;

	public FinalBlitPass(RenderPassEvent evt, Material blitMaterial)
	{
		m_BlitMaterial = blitMaterial;
		base.RenderPassEvent = evt;
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source)
	{
		m_Source = source;
		m_Dest = RenderTargetHandle.CameraTarget;
		m_Desc = baseDescriptor;
	}

	public void Setup(RenderTextureDescriptor baseDescriptor, in RenderTargetHandle source, in RenderTargetHandle dest)
	{
		m_Source = source;
		m_Dest = dest;
		m_Desc = baseDescriptor;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_BlitMaterial == null)
		{
			Debug.LogErrorFormat("Missing {0}. {1} render pass will not execute. Check for missing reference in the renderer resources.", m_BlitMaterial, GetType().Name);
			return;
		}
		bool num = m_Dest != RenderTargetHandle.CameraTarget;
		CommandBuffer commandBuffer = CommandBufferPool.Get("Final Blit Pass");
		if (num)
		{
			commandBuffer.EnableShaderKeyword(ShaderKeywordStrings._LINEAR_TO_SRGB_CONVERSION);
		}
		else
		{
			commandBuffer.DisableShaderKeyword(ShaderKeywordStrings._LINEAR_TO_SRGB_CONVERSION);
		}
		if (renderingData.CameraData.IsStereoEnabled || renderingData.CameraData.IsSceneViewCamera)
		{
			if (renderingData.ColorSpace == ColorSpace.Linear)
			{
				commandBuffer.GetTemporaryRT(m_GammaConvertSceneView.Id, m_Desc, FilterMode.Point);
				commandBuffer.SetRenderTarget(m_GammaConvertSceneView.Identifier());
				commandBuffer.SetGlobalTexture("_BlitTex", m_Source.Identifier());
				commandBuffer.Blit(m_Source.Identifier(), m_GammaConvertSceneView.Identifier(), m_BlitMaterial);
				commandBuffer.Blit(m_GammaConvertSceneView.Identifier(), BuiltinRenderTextureType.CameraTarget);
			}
			else
			{
				commandBuffer.Blit(m_Source.Identifier(), BuiltinRenderTextureType.CameraTarget);
			}
		}
		else
		{
			commandBuffer.SetGlobalTexture("_BlitTex", m_Source.Identifier());
			if (m_Dest == RenderTargetHandle.CameraTarget)
			{
				SetRenderTarget(commandBuffer, BuiltinRenderTextureType.CameraTarget, RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, ClearFlag.None, Color.black, m_Desc.dimension);
			}
			else
			{
				commandBuffer.SetRenderTarget(m_Dest.Identifier());
			}
			commandBuffer.SetViewProjectionMatrices(Matrix4x4.identity, Matrix4x4.identity);
			commandBuffer.SetViewport(renderingData.CameraData.Camera.pixelRect);
			commandBuffer.DrawMesh(RenderingUtils.FullscreenMesh, Matrix4x4.identity, m_BlitMaterial);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(m_GammaConvertSceneView.Id);
		base.FrameCleanup(cmd);
	}
}
