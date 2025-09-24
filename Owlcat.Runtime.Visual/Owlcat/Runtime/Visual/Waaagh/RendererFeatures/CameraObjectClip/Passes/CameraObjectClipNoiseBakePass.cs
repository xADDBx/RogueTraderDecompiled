using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip.Passes;

public class CameraObjectClipNoiseBakePass : ScriptableRenderPass<CameraObjectClipNoiseBakePassData>
{
	private static int _OccludedObjectNoiseMap3D = Shader.PropertyToID("_OccludedObjectNoiseMap3D");

	private CameraObjectClipFeature m_Feature;

	private Material m_NoiseBakeMaterial;

	public override string Name => "CameraObjectClipNoiseBakePass";

	public CameraObjectClipNoiseBakePass(RenderPassEvent evt, CameraObjectClipFeature feature, Material noiseBakeMaterial)
		: base(evt)
	{
		m_Feature = feature;
		m_NoiseBakeMaterial = noiseBakeMaterial;
	}

	protected override void Setup(RenderGraphBuilder builder, CameraObjectClipNoiseBakePassData data, ref RenderingData renderingData)
	{
		data.NoiseBakeMaterial = m_NoiseBakeMaterial;
		TextureHandle input = renderingData.RenderGraph.ImportTexture(m_Feature.Noise3D);
		data.Noise3DRT = builder.WriteTexture(in input);
		data.VolumeDepth = m_Feature.Noise3D.rt.volumeDepth;
		data.NoiseTiling = m_Feature.Settings.NoiseTiling;
	}

	protected override void Render(CameraObjectClipNoiseBakePassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalFloat("_NoiseTiling", data.NoiseTiling);
		int volumeDepth = data.VolumeDepth;
		for (int i = 0; i < volumeDepth; i++)
		{
			context.cmd.SetGlobalFloat("_NoiseSlice", i);
			context.cmd.Blit((Texture)null, data.Noise3DRT, data.NoiseBakeMaterial, 0, i);
		}
		context.cmd.SetGlobalTexture(_OccludedObjectNoiseMap3D, data.Noise3DRT);
	}
}
