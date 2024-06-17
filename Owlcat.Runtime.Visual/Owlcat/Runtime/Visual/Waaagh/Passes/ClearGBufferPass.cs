using UnityEngine;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public sealed class ClearGBufferPass : ScriptableRenderPass<ClearGBufferPassData>
{
	public override string Name => "ClearGBufferPass";

	public ClearGBufferPass(RenderPassEvent evt)
		: base(evt)
	{
	}

	protected override void Setup(RenderGraphBuilder builder, ClearGBufferPassData data, ref RenderingData renderingData)
	{
		builder.AllowRendererListCulling(!renderingData.IrsHasOpaques && !renderingData.IrsHasOpaqueDistortions);
		builder.DependsOn(in renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.OpaqueGBuffer.List);
		builder.DependsOn(in renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.OpaqueDistortionGBuffer.List);
		builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Write);
		data.CameraAlbedoRT = builder.UseColorBuffer(in data.Resources.CameraAlbedoRT, 0);
		data.CameraSpecularRT = builder.UseColorBuffer(in data.Resources.CameraSpecularRT, 1);
		data.CameraNormalsRT = builder.UseColorBuffer(in data.Resources.CameraNormalsRT, 2);
		data.CameraTranslucencyRT = builder.UseColorBuffer(in data.Resources.CameraTranslucencyRT, 3);
		data.CameraBakedGIRT = builder.UseColorBuffer(in data.Resources.CameraBakedGIRT, 4);
		data.CameraShadowmaskRT = builder.UseColorBuffer(in data.Resources.CameraShadowmaskRT, 5);
		data.ClearFlags = (renderingData.CameraData.IsLightingEnabled ? RTClearFlags.All : RTClearFlags.DepthStencil);
	}

	protected override void Render(ClearGBufferPassData data, RenderGraphContext context)
	{
		if (data.ClearFlags != 0)
		{
			context.cmd.ClearRenderTarget(data.ClearFlags, Color.clear, 1f, 128u);
		}
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraAlbedoRT, data.CameraAlbedoRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraSpecularRT, data.CameraSpecularRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, data.CameraBakedGIRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraShadowmaskRT, data.CameraShadowmaskRT);
	}
}
