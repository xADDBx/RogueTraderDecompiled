using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DrawObjectsPass : DrawRendererListPass<DrawObjectsPassData>
{
	public enum RendererListType
	{
		OpaqueDistortionForward,
		Transparent,
		Overlay
	}

	private const string kForwardLitPassName = "FORWARD LIT";

	private RendererListType m_RendererListType;

	private string m_Name;

	private RenderQueueRange m_RenderQueueRange;

	public override string Name => m_Name;

	public DrawObjectsPass(RenderPassEvent evt, RendererListType rendererListType)
		: base(evt)
	{
		m_RendererListType = rendererListType;
		m_Name = $"DrawObjects.{m_RendererListType}";
	}

	protected override void GetOrCreateRendererList(ref RenderingData renderingData, WaaaghRendererLists sharedRendererLists, out RendererListHandle rendererList)
	{
		switch (m_RendererListType)
		{
		case RendererListType.OpaqueDistortionForward:
			rendererList = sharedRendererLists.OpaqueDistortionForward.List;
			m_RenderQueueRange = sharedRendererLists.OpaqueDistortionForward.Desc.renderQueueRange;
			break;
		case RendererListType.Transparent:
			rendererList = sharedRendererLists.Transparent.List;
			m_RenderQueueRange = sharedRendererLists.Transparent.Desc.renderQueueRange;
			break;
		case RendererListType.Overlay:
			rendererList = sharedRendererLists.Overlay.List;
			m_RenderQueueRange = sharedRendererLists.Overlay.Desc.renderQueueRange;
			break;
		default:
			rendererList = sharedRendererLists.Transparent.List;
			m_RenderQueueRange = sharedRendererLists.Transparent.Desc.renderQueueRange;
			break;
		}
	}

	protected override void Setup(RenderGraphBuilder builder, DrawObjectsPassData data, ref RenderingData renderingData)
	{
		builder.UseColorBuffer(in data.Resources.CameraColorBuffer, 0);
		builder.UseDepthBuffer(in data.Resources.CameraDepthBuffer, DepthAccess.Read);
		if (m_RendererListType == RendererListType.OpaqueDistortionForward)
		{
			data.CameraNormalsRT = builder.ReadTexture(in data.Resources.CameraNormalsRT);
			data.CameraBakedGIRT = builder.ReadTexture(in data.Resources.CameraBakedGIRT);
			data.CameraShadowmaskRT = builder.ReadTexture(in data.Resources.CameraShadowmaskRT);
			data.CameraDepthCopyRT = builder.ReadTexture(in data.Resources.CameraDepthCopyRT);
		}
		if (m_RendererListType != RendererListType.Overlay && data.Resources.NativeShadowmap.IsValid())
		{
			TextureHandle input = data.Resources.NativeShadowmap;
			builder.ReadTexture(in input);
		}
		data.RenderQueueRange = m_RenderQueueRange;
		data.CameraType = renderingData.CameraData.CameraType;
		data.IsIndirectRenderingEnabled = renderingData.CameraData.IsIndirectRenderingEnabled;
		data.IsSceneViewInPrefabEditMode = renderingData.CameraData.IsSceneViewInPrefabEditMode;
		if (m_RendererListType == RendererListType.Transparent)
		{
			builder.AllowRendererListCulling(!renderingData.IrsHasTransparents);
		}
		else if (m_RendererListType == RendererListType.OpaqueDistortionForward)
		{
			builder.AllowRendererListCulling(!renderingData.IrsHasOpaqueDistortions);
		}
		else
		{
			builder.AllowRendererListCulling(value: true);
		}
	}

	protected override void Render(DrawObjectsPassData data, RenderGraphContext context)
	{
		context.cmd.DrawRendererList(data.RendererList);
		IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, "FORWARD LIT", data.RenderQueueRange);
	}
}
