using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Base;

internal class SetupFogPass : ScriptableRenderPass<SetupFogPass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public bool IsFogEnabled;

		public FogMode FogMode;

		public Fog FogVolume;
	}

	private const float kInvisibleFogEnd = 1000000f;

	private const float kInvisibleFogStart = 999999f;

	private static readonly Color s_InvisibleFogColor = new Color(1f, 0f, 1f, 1f);

	private static readonly float4 s_InvisibleFogParams = FogUtils.MakeFogLinearParams(999999f, 1000000f);

	public override string Name => "SetupFogPass";

	public SetupFogPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, PassData data, ref RenderingData renderingData)
	{
		data.IsFogEnabled = RenderSettings.fog;
		data.FogMode = RenderSettings.fogMode;
		data.FogVolume = VolumeManager.instance.stack.GetComponent<Fog>();
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		if (data.IsFogEnabled && data.FogMode == FogMode.Linear)
		{
			Color fogColor;
			float4 fogParams;
			if (data.FogVolume.IsActive())
			{
				fogColor = CoreUtils.ConvertSRGBToActiveColorSpace(data.FogVolume.Color.value);
				fogParams = FogUtils.MakeFogLinearParams(data.FogVolume.StartDistance.value, data.FogVolume.EndDistance.value);
			}
			else
			{
				fogColor = CoreUtils.ConvertSRGBToActiveColorSpace(RenderSettings.fogColor);
				fogParams = FogUtils.MakeFogLinearParams(RenderSettings.fogStartDistance, RenderSettings.fogEndDistance);
			}
			FogUtils.SetupFogProperties(context.cmd, in fogColor, in fogParams);
		}
		else
		{
			FogUtils.SetupFogMode(context.cmd, FogMode.Linear);
			FogUtils.SetupFogProperties(context.cmd, in s_InvisibleFogColor, in s_InvisibleFogParams);
		}
	}
}
