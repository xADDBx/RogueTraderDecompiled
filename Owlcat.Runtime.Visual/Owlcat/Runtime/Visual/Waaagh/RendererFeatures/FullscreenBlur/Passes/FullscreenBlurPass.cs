using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur.Passes;

public class FullscreenBlurPass : ScriptableRenderPass<FullscreenBlurPassData>
{
	private FullscreenBlurFeature m_Feature;

	private Material m_BlurMaterial;

	public override string Name => "FullscreenBlurPass";

	public FullscreenBlurPass(RenderPassEvent evt, FullscreenBlurFeature feature, Material blurMaterial)
		: base(evt)
	{
		m_Feature = feature;
		m_BlurMaterial = blurMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, FullscreenBlurPassData data, ref RenderingData renderingData)
	{
		data.Feature = m_Feature;
		data.BlurMaterial = m_BlurMaterial;
		data.CameraColorRT = builder.ReadWriteTexture(in data.Resources.CameraColorBuffer);
		float num = 1f / (float)m_Feature.Downsample;
		TextureDesc desc = RenderingUtils.CreateTextureDesc("BlurRT0", renderingData.CameraData.CameraTargetDescriptor);
		desc.width = (int)((float)desc.width * num);
		desc.height = (int)((float)desc.height * num);
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.depthBufferBits = DepthBits.None;
		data.BlurRT0 = builder.CreateTransientTexture(in desc);
		desc.name = "BlurRT1";
		data.BlurRT1 = builder.CreateTransientTexture(in desc);
	}

	protected override void Render(FullscreenBlurPassData data, RenderGraphContext context)
	{
		float num = 1f / (float)data.Feature.Downsample;
		context.cmd.SetGlobalVector("_Parameter", new Vector4(data.Feature.BlurSize * num, (0f - data.Feature.BlurSize) * num, 0f, 0f));
		context.cmd.Blit(data.CameraColorRT, data.BlurRT0);
		int num2 = ((data.Feature.BlurType != 0) ? 2 : 0);
		for (int i = 0; i < data.Feature.BlurIterations; i++)
		{
			float num3 = (float)i * 1f;
			context.cmd.SetGlobalVector("_Parameter", new Vector4(data.Feature.BlurSize * num + num3, (0f - data.Feature.BlurSize) * num - num3, 0f, 0f));
			context.cmd.Blit(data.BlurRT0, data.BlurRT1, data.BlurMaterial, 1 + num2);
			context.cmd.Blit(data.BlurRT1, data.BlurRT0, data.BlurMaterial, 2 + num2);
		}
		context.cmd.Blit(data.BlurRT0, data.CameraColorRT);
	}
}
