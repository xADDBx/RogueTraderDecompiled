using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghRendererLists
{
	public class WaaaghRendererList
	{
		public RendererListDesc Desc;

		public RendererListHandle List;
	}

	private ShaderTagId m_GBufferShaderTagId = new ShaderTagId("GBuffer");

	private ShaderTagId[] m_ForwardShaderTags = new ShaderTagId[2]
	{
		new ShaderTagId("SRPDefaultUnlit"),
		new ShaderTagId("ForwardLit")
	};

	private ShaderTagId m_DistortionVectorShaderTagId = new ShaderTagId("DistortionVectors");

	public WaaaghRendererList OpaqueGBuffer = new WaaaghRendererList();

	public WaaaghRendererList OpaqueDistortionGBuffer = new WaaaghRendererList();

	public WaaaghRendererList OpaqueDistortionForward = new WaaaghRendererList();

	public WaaaghRendererList Transparent = new WaaaghRendererList();

	public WaaaghRendererList DistortionVectors = new WaaaghRendererList();

	public WaaaghRendererList Overlay = new WaaaghRendererList();

	public void Init(ref RenderingData renderingData)
	{
		PerObjectData rendererConfiguration = PerObjectData.LightProbe | PerObjectData.Lightmaps | PerObjectData.ShadowMask;
		SortingCriteria sortingCriteria = SortingCriteria.SortingLayer | SortingCriteria.RenderQueue | SortingCriteria.OptimizeStateChanges | SortingCriteria.CanvasOrder;
		OpaqueGBuffer.Desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_GBufferShaderTagId, rendererConfiguration, WaaaghRenderQueue.OpaquePreDistortion, sortingCriteria);
		OpaqueGBuffer.List = renderingData.RenderGraph.CreateRendererList(in OpaqueGBuffer.Desc);
		OpaqueDistortionGBuffer.Desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_GBufferShaderTagId, rendererConfiguration, WaaaghRenderQueue.OpaqueDistortion, renderingData.CameraData.DefaultOpaqueSortFlags);
		OpaqueDistortionGBuffer.List = renderingData.RenderGraph.CreateRendererList(in OpaqueDistortionGBuffer.Desc);
		OpaqueDistortionForward.Desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_ForwardShaderTags, renderingData.PerObjectData, WaaaghRenderQueue.OpaqueDistortion);
		OpaqueDistortionForward.List = renderingData.RenderGraph.CreateRendererList(in OpaqueDistortionForward.Desc);
		Transparent.Desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_ForwardShaderTags, -1, renderingData.PerObjectData, WaaaghRenderQueue.Transparent, SortingCriteria.CommonTransparent);
		Transparent.List = renderingData.RenderGraph.CreateRendererList(in Transparent.Desc);
		DistortionVectors.Desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_DistortionVectorShaderTagId, PerObjectData.None, WaaaghRenderQueue.Transparent);
		DistortionVectors.List = renderingData.RenderGraph.CreateRendererList(in DistortionVectors.Desc);
		Overlay.Desc = RenderingUtils.CreateRendererListDesc(renderingData.CullingResults, renderingData.CameraData.Camera, m_ForwardShaderTags, -1, renderingData.PerObjectData, WaaaghRenderQueue.Overlay, SortingCriteria.CommonTransparent);
		Overlay.List = renderingData.RenderGraph.CreateRendererList(in Overlay.Desc);
	}
}
