using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class DebugLocalVolumetricFogPass : ScriptableRenderPass<DebugLocalVolumetricFogPassData>
{
	private Material m_DebugMaterial;

	private VolumetricLightingFeature m_Feature;

	public override string Name => "DebugLocalVolumetricFogPass";

	public DebugLocalVolumetricFogPass(RenderPassEvent evt, VolumetricLightingFeature feature, Material debugMaterial)
		: base(evt)
	{
		m_DebugMaterial = debugMaterial;
		m_Feature = feature;
	}

	protected override void Setup(RenderGraphBuilder builder, DebugLocalVolumetricFogPassData data, ref RenderingData renderingData)
	{
		data.Material = m_DebugMaterial;
		data.CameraColorBuffer = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		data.DepthCopyTexture = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		data.LocalFogClusteringParams = m_Feature.FogClusteringParams;
		data.FogTilesBuffer = builder.ReadComputeBuffer(in m_Feature.FogTilesBufferHandle);
		data.FogZBinsBuffer = builder.ReadComputeBuffer(in m_Feature.ZBinsBufferHandle);
	}

	protected override void Render(DebugLocalVolumetricFogPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalVector(ShaderPropertyId._LocalVolumetricFogClusteringParams, data.LocalFogClusteringParams);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._FogTilesBuffer, data.FogTilesBuffer);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._LocalFogZBinsBuffer, data.FogZBinsBuffer);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
