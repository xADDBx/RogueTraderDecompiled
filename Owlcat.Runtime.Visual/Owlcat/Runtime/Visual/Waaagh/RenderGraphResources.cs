using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Experimental.Rendering.RenderGraphModule;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class RenderGraphResources
{
	private RenderGraph m_RenderGraph;

	private TextureHandle m_CameraScaledColorBuffer;

	private TextureHandle m_CameraScaledDepthBuffer;

	private TextureHandle m_CameraNonScaledColorBuffer;

	private TextureHandle m_CameraNonScaledDepthBuffer;

	private TextureHandle m_CameraHistoryColorBuffer;

	private TextureHandle m_CameraHistoryDepthBuffer;

	private TextureHandle m_NativeShadowmap;

	private TextureHandle m_NativeCachedShadowmap;

	public TextureHandle FinalTargetDepth;

	public bool IsFinalTargetDepthHasDepthBits;

	public TextureHandle CameraResolveColorBuffer;

	public TextureHandle CameraColorBuffer;

	public TextureHandle CameraDepthBuffer;

	public TextureHandle CameraColorPyramidRT;

	public TextureHandle CameraDepthCopyRT;

	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraSpecularRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraTranslucencyRT;

	public TextureHandle ColorGradingLut;

	public TextureHandle BloomTexture;

	public TextureHandle CameraDepthPyramidRT;

	public TextureHandle SsrRT;

	public TextureHandle CameraMotionVectorsRT;

	public TextureHandle VolumetricScatter;

	public TextureHandle TilesMinMaxZTexture;

	public ComputeBufferHandle LightDataConstantBuffer;

	public ComputeBufferHandle LightVolumeDataConstantBuffer;

	public ComputeBufferHandle ZBinsConstantBuffer;

	public ComputeBufferHandle LightTilesBuffer;

	public WaaaghRendererLists RendererLists = new WaaaghRendererLists();

	public RenderGraph RenderGraph => m_RenderGraph;

	public TextureHandle FinalTarget { get; private set; }

	public TextureHandle CameraHistoryColorBuffer => m_CameraHistoryColorBuffer;

	public TextureHandle CameraHistoryDepthBuffer => m_CameraHistoryDepthBuffer;

	public TextureHandle NativeShadowmap => m_NativeShadowmap;

	public TextureHandle NativeCachedShadowmap => m_NativeCachedShadowmap;

	internal RenderGraphResources()
	{
	}

	internal void SetRenderGraph(RenderGraph renderGraph)
	{
		m_RenderGraph = renderGraph;
	}

	public void ImportCameraData(ref CameraData cameraData)
	{
		TextureDesc desc = RenderingUtils.CreateTextureDesc(null, cameraData.CameraTargetDescriptor);
		desc.depthBufferBits = DepthBits.None;
		desc.filterMode = FilterMode.Bilinear;
		desc.wrapMode = TextureWrapMode.Clamp;
		desc.enableRandomWrite = true;
		TextureDesc desc2 = RenderingUtils.CreateTextureDesc(null, cameraData.CameraTargetDescriptor);
		desc2.colorFormat = GraphicsFormat.D24_UNorm_S8_UInt;
		desc2.depthBufferBits = DepthBits.Depth32;
		desc2.filterMode = FilterMode.Point;
		Vector2Int nonScaledCameraTargetViewportSize = cameraData.NonScaledCameraTargetViewportSize;
		Vector2Int scaledCameraTargetViewportSize = cameraData.ScaledCameraTargetViewportSize;
		desc.name = "CameraColor";
		desc.width = nonScaledCameraTargetViewportSize.x;
		desc.height = nonScaledCameraTargetViewportSize.y;
		m_CameraNonScaledColorBuffer = m_RenderGraph.CreateTexture(in desc);
		desc2.name = "CameraDepth";
		desc2.width = nonScaledCameraTargetViewportSize.x;
		desc2.height = nonScaledCameraTargetViewportSize.y;
		m_CameraNonScaledDepthBuffer = m_RenderGraph.CreateTexture(in desc2);
		desc.name = "CameraColorScaled";
		desc.width = scaledCameraTargetViewportSize.x;
		desc.height = scaledCameraTargetViewportSize.y;
		m_CameraScaledColorBuffer = m_RenderGraph.CreateTexture(in desc);
		desc2.name = "CameraDepthScaled";
		desc2.width = scaledCameraTargetViewportSize.x;
		desc2.height = scaledCameraTargetViewportSize.y;
		m_CameraScaledDepthBuffer = m_RenderGraph.CreateTexture(in desc2);
		FinalTarget = ((cameraData.TargetTexture != null) ? m_RenderGraph.ImportBackbuffer(new RenderTargetIdentifier(cameraData.TargetTexture)) : m_RenderGraph.ImportBackbuffer(BuiltinRenderTextureType.CameraTarget));
		if (cameraData.TargetDepthTexture != null)
		{
			FinalTargetDepth = m_RenderGraph.ImportTexture(cameraData.CameraBuffer.CameraTargetDepthTexture);
			IsFinalTargetDepthHasDepthBits = cameraData.TargetDepthTexture.depth != 0;
		}
		else if (cameraData.TargetTexture != null)
		{
			FinalTargetDepth = FinalTarget;
			IsFinalTargetDepthHasDepthBits = cameraData.TargetTexture.depth != 0;
		}
		else
		{
			FinalTargetDepth = FinalTarget;
			IsFinalTargetDepthHasDepthBits = true;
		}
		switch (cameraData.CameraRenderTargetBufferType)
		{
		case CameraRenderTargetType.Scaled:
			CameraColorBuffer = m_CameraScaledColorBuffer;
			CameraDepthBuffer = m_CameraScaledDepthBuffer;
			break;
		default:
			CameraColorBuffer = m_CameraNonScaledColorBuffer;
			CameraDepthBuffer = m_CameraNonScaledDepthBuffer;
			break;
		}
		switch (cameraData.CameraResolveTargetBufferType)
		{
		case CameraResolveTargetType.NonScaled:
			CameraResolveColorBuffer = m_CameraNonScaledColorBuffer;
			break;
		case CameraResolveTargetType.Backbuffer:
			CameraResolveColorBuffer = FinalTarget;
			break;
		default:
			CameraResolveColorBuffer = TextureHandle.nullHandle;
			break;
		}
		m_CameraHistoryColorBuffer = m_RenderGraph.ImportTexture(cameraData.CameraBuffer.GetCurrentFrameRT(WaaaghCameraBuffer.HistoryType.Color));
		m_CameraHistoryDepthBuffer = m_RenderGraph.ImportTexture(cameraData.CameraBuffer.GetCurrentFrameRT(WaaaghCameraBuffer.HistoryType.Depth));
	}

	public void Cleanup()
	{
	}

	internal void ImportShadowmap(ref ShadowData shadowData)
	{
		m_NativeShadowmap = ((shadowData.ShadowManager.ShadowMapAtlas != null) ? m_RenderGraph.ImportTexture(shadowData.ShadowManager.ShadowMapAtlas.Texture) : TextureHandle.nullHandle);
		m_NativeCachedShadowmap = ((shadowData.StaticShadowsCacheEnabled && shadowData.ShadowManager.CachedShadowMapAtlas != null) ? m_RenderGraph.ImportTexture(shadowData.ShadowManager.CachedShadowMapAtlas.Texture) : TextureHandle.nullHandle);
	}
}
