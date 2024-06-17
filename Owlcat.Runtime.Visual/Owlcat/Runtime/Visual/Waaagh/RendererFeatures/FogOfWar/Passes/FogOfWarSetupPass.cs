using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarSetupPass : ScriptableRenderPass<FogOfWarSetupPassData>
{
	private FogOfWarArea m_Area;

	private FogOfWarSettings m_Settings;

	private FogOfWarFeature m_Feature;

	public override string Name => "FogOfWarSetupPass";

	public FogOfWarSetupPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	internal void Init(FogOfWarArea activeArea, FogOfWarFeature feature, FogOfWarSettings settings)
	{
		m_Area = activeArea;
		m_Settings = settings;
		m_Feature = feature;
	}

	protected override void Setup(RenderGraphBuilder builder, FogOfWarSetupPassData data, ref RenderingData renderingData)
	{
		data.Area = m_Area;
		data.Settings = m_Settings;
		if (m_Area == null || !m_Area.isActiveAndEnabled)
		{
			data.DefaultFogOfWarMask = m_Feature.DefaultFogOfWarMask;
			return;
		}
		data.MaskSize.x = data.Area.FogOfWarMapRT.rt.width;
		data.MaskSize.y = data.Area.FogOfWarMapRT.rt.height;
	}

	protected override void Render(FogOfWarSetupPassData data, RenderGraphContext context)
	{
		if (data.Area == null || !data.Area.isActiveAndEnabled)
		{
			Shader.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, 0f);
			Shader.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, data.DefaultFogOfWarMask);
			return;
		}
		context.cmd.SetGlobalTexture(FogOfWarConstantBuffer._FogOfWarMask, data.Area.FogOfWarMapRT);
		context.cmd.SetGlobalVector(FogOfWarConstantBuffer._FogOfWarMask_ST, data.Area.CalculateMaskST());
		context.cmd.SetGlobalColor(FogOfWarConstantBuffer._FogOfWarColor, data.Settings.Color);
		context.cmd.SetGlobalFloat(FogOfWarConstantBuffer._FogOfWarGlobalFlag, (!data.Area.ApplyShaderManually) ? 1 : 0);
		context.cmd.SetGlobalVector(FogOfWarConstantBuffer._FogOfWarMaskSize, data.MaskSize);
	}
}
