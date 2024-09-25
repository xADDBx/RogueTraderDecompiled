using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CameraObjectClip.Passes;

public class CameraObjectClipSetupPass : ScriptableRenderPass<CameraObjectClipSetupPassData>
{
	private CameraObjectClipFeature m_Feature;

	public override string Name => "CameraObjectClipSetupPass";

	public CameraObjectClipSetupPass(RenderPassEvent evt, CameraObjectClipFeature feature)
		: base(evt)
	{
		m_Feature = feature;
	}

	protected override void Setup(RenderGraphBuilder builder, CameraObjectClipSetupPassData data, ref RenderingData renderingData)
	{
		data.Feature = m_Feature;
	}

	protected override void Render(CameraObjectClipSetupPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectAlphaScale, data.Feature.Settings.AlphaScale);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectClipNoiseTiling, data.Feature.Settings.NoiseTiling);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectClipTreshold, data.Feature.Settings.ClipTreshold);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectClipNearCameraDistance, data.Feature.Settings.NearCameraClipDistance);
		context.cmd.SetGlobalFloat(ShaderPropertyId._OccludedObjectHighlightingFeatureEnabled, 1f);
	}
}
