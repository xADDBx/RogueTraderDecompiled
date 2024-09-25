using Owlcat.Runtime.Visual.Waaagh.Debugging;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public class ApplyDebugSettingsPass : ScriptableRenderPass<ApplyDebugSettingsPassData>
{
	private DebugHandler.DebugMipMapTexture m_MipMapTexture;

	public override string Name => "ApplyDebugSettingsPass";

	internal ApplyDebugSettingsPass(RenderPassEvent evt, DebugHandler.DebugMipMapTexture mipMapTexture)
		: base(evt)
	{
		m_MipMapTexture = mipMapTexture;
	}

	protected override void Setup(RenderGraphBuilder builder, ApplyDebugSettingsPassData data, ref RenderingData renderingData)
	{
		WaaaghDebugData debugData = WaaaghPipeline.Asset.DebugData;
		data.DebugData = debugData;
		data.MipMapTexture = m_MipMapTexture.MipMapTexture;
	}

	protected override void Render(ApplyDebugSettingsPassData data, RenderGraphContext context)
	{
		bool flag = data.DebugData?.DebugNeedsDebugDisplayKeyword() ?? false;
		CoreUtils.SetKeyword(context.cmd, ShaderKeywordStrings.DEBUG_DISPLAY, flag);
		if (flag)
		{
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugMaterialMode, (int)data.DebugData.RenderingDebug.DebugMaterialMode);
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugLightingMode, (int)data.DebugData.LightingDebug.DebugLightingMode);
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugOverdrawMode, (int)data.DebugData.RenderingDebug.OverdrawMode);
			context.cmd.SetGlobalInt(ShaderPropertyId._DebugMipMap, data.DebugData.RenderingDebug.DebugMipMap ? 1 : 0);
			context.cmd.SetGlobalTexture(ShaderPropertyId._MipMapDebugMap, data.MipMapTexture);
		}
	}
}
