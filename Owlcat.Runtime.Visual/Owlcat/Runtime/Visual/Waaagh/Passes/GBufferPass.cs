using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class GBufferPass : DrawRendererListPass<GBufferPassData>
{
	private const string kGBufferPassName = "GBUFFER";

	private string m_Name;

	private RenderQueueRange m_RenderQueueRange;

	private GBufferType m_Type;

	public override string Name => m_Name;

	public GBufferPass(RenderPassEvent evt, GBufferType gBufferType)
		: base(evt)
	{
		m_Name = string.Format("{0}.{1}", "GBufferPass", gBufferType);
		m_Type = gBufferType;
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererList rendererList)
	{
		switch (m_Type)
		{
		case GBufferType.Opaque:
			rendererList = sharedRendererLists.OpaqueGBuffer.List;
			m_RenderQueueRange = sharedRendererLists.OpaqueGBuffer.Desc.renderQueueRange;
			break;
		case GBufferType.OpaqueDistortion:
			rendererList = sharedRendererLists.OpaqueDistortionGBuffer.List;
			m_RenderQueueRange = sharedRendererLists.OpaqueDistortionGBuffer.Desc.renderQueueRange;
			break;
		default:
			rendererList = sharedRendererLists.OpaqueGBuffer.List;
			m_RenderQueueRange = sharedRendererLists.OpaqueGBuffer.Desc.renderQueueRange;
			break;
		}
	}

	protected override void Setup(RenderGraphBuilder builder, GBufferPassData data, ref RenderingData renderingData)
	{
		data.CameraDepthBuffer = builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.ReadWrite);
		data.CameraAlbedoRT = builder.UseColorBuffer(in data.Resources.CameraAlbedoRT, 0);
		data.CameraSpecularRT = builder.UseColorBuffer(in data.Resources.CameraSpecularRT, 1);
		data.CameraNormalsRT = builder.UseColorBuffer(in data.Resources.CameraNormalsRT, 2);
		data.CameraEmissionRT = builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 3);
		data.CameraTranslucencyRT = builder.UseColorBuffer(in data.Resources.CameraTranslucencyRT, 4);
		data.CameraBakedGIRT = builder.UseColorBuffer(in data.Resources.CameraBakedGIRT, 5);
		data.CameraShadowmaskRT = builder.UseColorBuffer(in data.Resources.CameraShadowmaskRT, 6);
		data.RenderQueueRange = m_RenderQueueRange;
		data.CameraType = renderingData.CameraData.CameraType;
		data.IsIndirectRenderingEnabled = renderingData.CameraData.IsIndirectRenderingEnabled;
		data.IsSceneViewInPrefabEditMode = renderingData.CameraData.IsSceneViewInPrefabEditMode;
		if (m_Type == GBufferType.Opaque)
		{
			builder.AllowRendererListCulling(!renderingData.IrsHasOpaques);
		}
		else
		{
			builder.AllowRendererListCulling(!renderingData.IrsHasOpaqueDistortions);
		}
	}

	protected override void Render(GBufferPassData data, RenderGraphContext context)
	{
		context.cmd.DrawRendererList(data.RendererList);
		IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, "GBUFFER", data.RenderQueueRange);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraAlbedoRT, data.CameraAlbedoRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraSpecularRT, data.CameraSpecularRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsTexture, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, data.CameraBakedGIRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraShadowmaskRT, data.CameraShadowmaskRT);
	}
}
