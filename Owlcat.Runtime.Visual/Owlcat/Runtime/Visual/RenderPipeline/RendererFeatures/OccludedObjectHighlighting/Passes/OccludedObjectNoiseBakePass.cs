using Owlcat.Runtime.Visual.RenderPipeline.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.RenderPipeline.RendererFeatures.OccludedObjectHighlighting.Passes;

public class OccludedObjectNoiseBakePass : ScriptableRenderPass
{
	private const string kProfilerTag = "Occluded Objects Bake Noise Texture";

	private ProfilingSampler m_ProfilingSampler = new ProfilingSampler("Occluded Objects Bake Noise Texture");

	private Material m_Material;

	private OccludedObjectHighlightingFeature m_Feature;

	public OccludedObjectNoiseBakePass(RenderPassEvent evt, Material material)
	{
		base.RenderPassEvent = evt;
		m_Material = material;
	}

	public void Setup(OccludedObjectHighlightingFeature feature)
	{
		m_Feature = feature;
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (m_Feature == null || m_Feature.Noise3D == null)
		{
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, m_ProfilingSampler))
		{
			commandBuffer.SetGlobalFloat("_NoiseTiling", m_Feature.DepthClip.NoiseTiling);
			int volumeDepth = m_Feature.Noise3D.volumeDepth;
			for (int i = 0; i < volumeDepth; i++)
			{
				commandBuffer.SetGlobalFloat("_NoiseSlice", i);
				commandBuffer.Blit((Texture)null, m_Feature.Noise3D, m_Material, 0, i);
			}
			commandBuffer.SetGlobalTexture("_OccludedObjectNoiseMap3D", m_Feature.Noise3D);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
