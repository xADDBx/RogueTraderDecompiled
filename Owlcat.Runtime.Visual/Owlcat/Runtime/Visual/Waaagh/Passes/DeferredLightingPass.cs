using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredLightingPass : ScriptableRenderPass<DeferredLightingPassData>
{
	private Material m_DeferredReflectionsMaterial;

	private Material m_DeferredLightingMaterial;

	public override string Name => "DeferredLightingPass";

	public DeferredLightingPass(RenderPassEvent evt, Material deferredReflectionsMaterial, Material deferredLightingMaterial)
		: base(evt)
	{
		m_DeferredReflectionsMaterial = deferredReflectionsMaterial;
		m_DeferredLightingMaterial = deferredLightingMaterial;
	}

	public override void ConfigureRendererLists(ref RenderingData renderingData, RenderGraphResources resources)
	{
		DependsOn(in resources.RendererLists.OpaqueGBuffer.List);
	}

	protected override void Setup(RenderGraphBuilder builder, DeferredLightingPassData data, ref RenderingData renderingData)
	{
		data.CameraColorRT = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		data.CameraDepthRT = builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Read);
		data.CameraDepthCopytRT = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		data.CameraAlbedoRT = builder.ReadTexture(in data.Resources.CameraAlbedoRT);
		data.CameraNormalsRT = builder.ReadTexture(in data.Resources.CameraNormalsRT);
		data.CameraBakedGIRT = builder.ReadTexture(in data.Resources.CameraBakedGIRT);
		data.CameraShadowmaskRT = builder.ReadTexture(in data.Resources.CameraShadowmaskRT);
		data.CameraTranslucencyRT = builder.ReadTexture(in data.Resources.CameraTranslucencyRT);
		if (data.Resources.NativeShadowmap.IsValid())
		{
			TextureHandle input = data.Resources.NativeShadowmap;
			builder.ReadTexture(in input);
		}
		data.VisibleReflectionProbes = renderingData.CullingResults.visibleReflectionProbes;
		data.DeferredReflectionsMaterial = m_DeferredReflectionsMaterial;
		data.DeferredLightingMaterial = m_DeferredLightingMaterial;
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		Color glossyEnvironmentColor = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
		data.GlossyEnvironmentColor = glossyEnvironmentColor;
		data.GlossyBlackColor = default(Color);
		builder.AllowRendererListCulling(!renderingData.IrsHasOpaques);
	}

	protected override void Render(DeferredLightingPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, data.GlossyBlackColor);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredLightingMaterial, 0, MeshTopology.Triangles, 3);
		context.cmd.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, data.GlossyEnvironmentColor);
	}
}
