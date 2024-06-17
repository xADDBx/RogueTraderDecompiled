using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Unity.Mathematics;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class SetupLightDataPass : ScriptableRenderPass<SetupLightDataPassData>
{
	private WaaaghLights m_WaaaghLights;

	public override string Name => "SetupLightDataPass";

	public SetupLightDataPass(RenderPassEvent evt, WaaaghLights waaaghLights)
		: base(evt)
	{
		m_WaaaghLights = waaaghLights;
	}

	protected override void Setup(RenderGraphBuilder builder, SetupLightDataPassData data, ref RenderingData renderingData)
	{
		data.LightDataConstantBufferHandle = builder.WriteComputeBuffer(in data.Resources.LightDataConstantBuffer);
		data.LightVolumeDataConstantBufferHandle = builder.WriteComputeBuffer(in data.Resources.LightVolumeDataConstantBuffer);
		data.ZBinsConstantBufferHandle = builder.WriteComputeBuffer(in data.Resources.ZBinsConstantBuffer);
		data.LightDataRaw = m_WaaaghLights.LightDataRaw;
		data.LightVolumeDataRaw = m_WaaaghLights.LightVolumeDataRaw;
		data.ZBins = m_WaaaghLights.ZBins;
		data.ClusteringParams = m_WaaaghLights.ClusteringParams;
		data.LightDataParams = m_WaaaghLights.LightDataParams;
	}

	protected override void Render(SetupLightDataPassData data, RenderGraphContext context)
	{
		context.cmd.SetBufferData(data.LightDataConstantBufferHandle, data.LightDataRaw);
		context.cmd.SetBufferData(data.LightVolumeDataConstantBufferHandle, data.LightVolumeDataRaw);
		context.cmd.SetBufferData(data.ZBinsConstantBufferHandle, data.ZBins.Reinterpret<float4>(Marshal.SizeOf<ZBin>()), 0, 0, 1024);
		context.cmd.SetGlobalConstantBuffer(data.LightDataConstantBufferHandle, ShaderPropertyId.LightDataConstantBuffer, 0, data.LightDataRaw.Length * 4 * 4);
		context.cmd.SetGlobalConstantBuffer(data.LightVolumeDataConstantBufferHandle, ShaderPropertyId.LightVolumeDataCB, 0, data.LightVolumeDataRaw.Length * 4 * 4);
		context.cmd.SetGlobalConstantBuffer(data.ZBinsConstantBufferHandle, ShaderPropertyId.ZBinsCB, 0, 16384);
		context.cmd.SetGlobalVector(ShaderPropertyId._LightDataParams, data.LightDataParams);
		context.cmd.SetGlobalVector(ShaderPropertyId._ClusteringParams, data.ClusteringParams);
	}
}
