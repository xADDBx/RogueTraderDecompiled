using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public class WaaaghCameraBuffer
{
	public enum HistoryType
	{
		Color,
		Depth,
		SSR
	}

	private Vector2 m_Jitter;

	private Vector2 m_JitterUV;

	private BufferedRTHandleSystem m_HistoryRTSystem = new BufferedRTHandleSystem();

	private int m_HistoryColorFramesCount;

	private int m_HistoryDepthFramesCount;

	private float m_PrevAspectRatio;

	private float m_SsrScale = 1f;

	private GraphicsFormat m_SsrColorFormat;

	internal bool m_TaaEnabled;

	public readonly Camera Camera;

	public readonly Vector2Int CameraRenderPixelSize;

	public RTHandle CameraTargetDepthTexture;

	internal int LastFrameId;

	internal int UnusedFramesCount;

	public Matrix4x4 JitterMatrix;

	public Matrix4x4 PreviuosViewProjectionMatrix;

	public int SampleIndex { get; private set; }

	public bool TaaEnabled => m_TaaEnabled;

	public Vector2 Jitter => m_Jitter;

	public Vector2 JitterUV => m_JitterUV;

	public int HistoryColorFramesCount => m_HistoryColorFramesCount;

	public int HistoryDepthFramesCount => m_HistoryDepthFramesCount;

	internal WaaaghCameraBuffer(Camera camera, Vector2Int cameraRenderPixelSize, int historyColorFramesCount, int historyDepthFramesCount, bool taaEnabled)
	{
		Camera = camera;
		CameraRenderPixelSize = cameraRenderPixelSize;
		m_HistoryColorFramesCount = historyColorFramesCount;
		m_HistoryDepthFramesCount = historyDepthFramesCount;
		m_TaaEnabled = taaEnabled;
		if (m_HistoryColorFramesCount > 0)
		{
			m_HistoryRTSystem.AllocBuffer(0, AllocColorHistoryBuffer, m_HistoryColorFramesCount);
		}
		if (m_HistoryDepthFramesCount > 0)
		{
			m_HistoryRTSystem.AllocBuffer(1, AllocDepthHistoryBuffer, m_HistoryDepthFramesCount);
		}
		if (Camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component) && component.TargetDepthTexture != null)
		{
			CameraTargetDepthTexture = RTHandles.Alloc(component.TargetDepthTexture);
		}
	}

	private RTHandle AllocDepthHistoryBuffer(RTHandleSystem rts, int frameIndex)
	{
		return rts.Alloc(CameraRenderPixelSize.x, CameraRenderPixelSize.y, 1, DepthBits.None, GraphicsFormat.R32_SFloat, FilterMode.Point, TextureWrapMode.Clamp, TextureDimension.Tex2D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: false, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, RenderTextureMemoryless.None, VRTextureUsage.None, $"{Camera.name}_HistoryDepth{frameIndex}");
	}

	private RTHandle AllocColorHistoryBuffer(RTHandleSystem rts, int frameIndex)
	{
		bool needsAlpha = false;
		bool isHdrEnabled = Camera.allowHDR && WaaaghPipeline.Asset.SupportsHDR;
		HDRColorBufferPrecision hDRColorBufferPrecision = WaaaghPipeline.Asset.HDRColorBufferPrecision;
		GraphicsFormat colorFormat = WaaaghPipeline.MakeRenderTextureGraphicsFormat(isHdrEnabled, hDRColorBufferPrecision, needsAlpha);
		return rts.Alloc(CameraRenderPixelSize.x, CameraRenderPixelSize.y, 1, DepthBits.None, colorFormat, FilterMode.Bilinear, TextureWrapMode.Clamp, TextureDimension.Tex2D, enableRandomWrite: false, useMipMap: false, autoGenerateMips: false, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, RenderTextureMemoryless.None, VRTextureUsage.None, $"{Camera.name}_HistoryColor{frameIndex}");
	}

	private int2 GetSsrSize()
	{
		return new int2((int)((float)CameraRenderPixelSize.x * m_SsrScale), (int)((float)CameraRenderPixelSize.y * m_SsrScale));
	}

	private RTHandle AllocSSRHistoryBuffer(RTHandleSystem rts, int frameIndex)
	{
		int2 ssrSize = GetSsrSize();
		return rts.Alloc(ssrSize.x, ssrSize.y, 1, DepthBits.None, m_SsrColorFormat, FilterMode.Bilinear, TextureWrapMode.Repeat, TextureDimension.Tex2D, enableRandomWrite: true, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, RenderTextureMemoryless.None, VRTextureUsage.None, $"{Camera.name}_HistorySSR{frameIndex}");
	}

	public RTHandle GetCurrentFrameRT(HistoryType historyType)
	{
		return m_HistoryRTSystem.GetFrameRT((int)historyType, 0);
	}

	public RTHandle GetPreviousFrameRT(HistoryType historyType)
	{
		return m_HistoryRTSystem.GetFrameRT((int)historyType, 1);
	}

	internal void AllocSsrHistoryBuffer(ScreenSpaceReflectionsQuality quality, ColorPrecision colorPrecision, int frameCount)
	{
		switch (quality)
		{
		case ScreenSpaceReflectionsQuality.None:
			throw new ArgumentException("You are trying to allocate SsrHistoryBuffer while SSR turned off.");
		case ScreenSpaceReflectionsQuality.Half:
			m_SsrScale = 0.5f;
			break;
		case ScreenSpaceReflectionsQuality.Full:
			m_SsrScale = 1f;
			break;
		}
		switch (colorPrecision)
		{
		case ColorPrecision.Color64:
			m_SsrColorFormat = GraphicsFormat.R16G16B16A16_SFloat;
			break;
		case ColorPrecision.Color32:
			m_SsrColorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			break;
		default:
			m_SsrColorFormat = GraphicsFormat.R16G16B16A16_SFloat;
			break;
		}
		RTHandle currentFrameRT = GetCurrentFrameRT(HistoryType.SSR);
		int2 ssrSize = GetSsrSize();
		if (currentFrameRT == null || currentFrameRT.rt.width != ssrSize.x || currentFrameRT.rt.height != ssrSize.y || currentFrameRT.rt.graphicsFormat != m_SsrColorFormat)
		{
			if (currentFrameRT != null)
			{
				m_HistoryRTSystem.ReleaseBuffer(2);
			}
			m_HistoryRTSystem.AllocBuffer(2, AllocSSRHistoryBuffer, frameCount);
		}
	}

	internal void Dispose()
	{
		m_HistoryRTSystem.Dispose();
		if (CameraTargetDepthTexture != null)
		{
			CameraTargetDepthTexture.Release();
		}
	}

	internal void Update()
	{
		UnusedFramesCount = 0;
		UpdateJitterMatrix();
		m_HistoryRTSystem.SwapAndSetReferenceSize(CameraRenderPixelSize.x, CameraRenderPixelSize.y);
	}

	private void UpdateJitterMatrix()
	{
		JitterMatrix = Matrix4x4.identity;
		if (TaaEnabled)
		{
			int frameCount = Time.frameCount;
			float num = CameraRenderPixelSize.x;
			float num2 = CameraRenderPixelSize.y;
			Vector2 vector = CalculateJitter(frameCount);
			float x = vector.x * (2f / num);
			float y = vector.y * (2f / num2);
			JitterMatrix = Matrix4x4.Translate(new Vector3(x, y, 0f));
		}
	}

	internal static Vector2 CalculateJitter(int frameIndex)
	{
		float x = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 2) - 0.5f;
		float y = HaltonSequence.Get((frameIndex & 0x3FF) + 1, 3) - 0.5f;
		return new Vector2(x, y);
	}

	internal void PostRender(ref CameraData cameraData)
	{
		bool flag = cameraData.FinalTargetAspectRatio != m_PrevAspectRatio;
		if (LastFrameId != FrameId.FrameCount || flag)
		{
			Matrix4x4 previuosViewProjectionMatrix = cameraData.GetGPUProjectionMatrixNoJitter() * cameraData.GetViewMatrix();
			PreviuosViewProjectionMatrix = previuosViewProjectionMatrix;
			m_PrevAspectRatio = cameraData.FinalTargetAspectRatio;
			LastFrameId = FrameId.FrameCount;
		}
	}

	internal bool CheckResolution(Vector2Int cameraRenderPixelSize)
	{
		if (m_HistoryColorFramesCount > 0)
		{
			RenderTexture rt = GetCurrentFrameRT(HistoryType.Color).rt;
			if (rt.width == cameraRenderPixelSize.x)
			{
				return rt.height == cameraRenderPixelSize.y;
			}
			return false;
		}
		if (m_HistoryDepthFramesCount > 0)
		{
			RenderTexture rt2 = GetCurrentFrameRT(HistoryType.Depth).rt;
			if (rt2.width == cameraRenderPixelSize.x)
			{
				return rt2.height == cameraRenderPixelSize.y;
			}
			return false;
		}
		return true;
	}
}
