using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.FullscreenBlur.Passes;

public class FullscreenBlurPass : ScriptableRenderPass
{
	private const string kProfilerTag = "Fullscreen Blur Pass";

	private static int m_TempRt0 = Shader.PropertyToID("_FullscreenBlurRT0");

	private static int m_TempRt1 = Shader.PropertyToID("_FullscreenBlurRT1");

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Fullscreen Blur Pass");

	private Material m_Material;

	private RenderTargetHandle m_CameraColorHandle;

	private FullscreenBlurFeature m_Feature;

	public FullscreenBlurPass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
	}

	internal void Setup(FullscreenBlurFeature feature, RenderTargetHandle cameraColorHandle)
	{
		m_Feature = feature;
		m_CameraColorHandle = cameraColorHandle;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			float num = 1f / (float)m_Feature.Downsample;
			commandBuffer.SetGlobalVector("_Parameter", new Vector4(m_Feature.BlurSize * num, (0f - m_Feature.BlurSize) * num, 0f, 0f));
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.CameraData.CameraTargetDescriptor;
			cameraTargetDescriptor.width = (int)((float)cameraTargetDescriptor.width * num);
			cameraTargetDescriptor.height = (int)((float)cameraTargetDescriptor.height * num);
			commandBuffer.GetTemporaryRT(m_TempRt0, cameraTargetDescriptor);
			commandBuffer.GetTemporaryRT(m_TempRt1, cameraTargetDescriptor);
			commandBuffer.Blit(m_CameraColorHandle.Identifier(), m_TempRt0);
			int num2 = ((m_Feature.BlurType != 0) ? 2 : 0);
			for (int i = 0; i < m_Feature.BlurIterations; i++)
			{
				float num3 = (float)i * 1f;
				commandBuffer.SetGlobalVector("_Parameter", new Vector4(m_Feature.BlurSize * num + num3, (0f - m_Feature.BlurSize) * num - num3, 0f, 0f));
				commandBuffer.Blit(m_TempRt0, m_TempRt1, m_Material, 1 + num2);
				commandBuffer.Blit(m_TempRt1, m_TempRt0, m_Material, 2 + num2);
			}
			commandBuffer.Blit(m_TempRt0, m_CameraColorHandle.Identifier());
			commandBuffer.ReleaseTemporaryRT(m_TempRt0);
			commandBuffer.ReleaseTemporaryRT(m_TempRt1);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
