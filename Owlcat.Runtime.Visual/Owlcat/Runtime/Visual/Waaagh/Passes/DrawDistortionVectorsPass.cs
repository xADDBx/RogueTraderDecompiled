using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawDistortionVectorsPass : DrawRendererListPass<DrawDistortionVectorsPassData>
{
	private ShaderTagId m_ShaderTagId = new ShaderTagId("DistortionVectors");

	private Material m_ApplyDistortionMaterial;

	public override string Name => "DrawDistortionVectorsPass";

	public DrawDistortionVectorsPass(RenderPassEvent evt, Material applyDistortionMaterial)
		: base(evt)
	{
		m_ApplyDistortionMaterial = applyDistortionMaterial;
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList)
	{
		rendererList = renderingData.CameraData.Renderer.RenderGraphResources.RendererLists.DistortionVectors.List;
	}

	protected override void Setup(RenderGraphBuilder builder, DrawDistortionVectorsPassData data, ref RenderingData renderingData)
	{
		TextureDesc desc = RenderingUtils.CreateTextureDesc("DistortionRT", renderingData.CameraData.CameraTargetDescriptor);
		desc.depthBufferBits = DepthBits.None;
		desc.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
		data.DistortionRT = builder.CreateTransientTexture(in desc);
		data.CameraColorPyramidRT = builder.ReadTexture(in data.Resources.CameraColorPyramidRT);
		data.CameraColorRT = builder.WriteTexture(in data.Resources.CameraColorBuffer);
		data.CameraDepthRT = builder.ReadTexture(in data.Resources.CameraDepthBuffer);
		data.ApplyDistortionMaterial = m_ApplyDistortionMaterial;
	}

	protected override void Render(DrawDistortionVectorsPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.DistortionRT, data.CameraDepthRT);
		context.cmd.ClearRenderTarget(clearDepth: false, clearColor: true, data.ClearColor);
		context.cmd.DrawRendererList(data.RendererList);
		context.cmd.SetGlobalTexture(ShaderPropertyId._DistortionVectorsRT, data.DistortionRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraColorPyramidRT, data.CameraColorPyramidRT);
		context.cmd.Blit(data.DistortionRT, data.CameraColorRT, data.ApplyDistortionMaterial, 0);
	}
}
