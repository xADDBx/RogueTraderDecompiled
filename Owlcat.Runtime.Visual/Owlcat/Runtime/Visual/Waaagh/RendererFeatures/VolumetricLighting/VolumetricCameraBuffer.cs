using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

public class VolumetricCameraBuffer
{
	private Camera m_Camera;

	private Vector2Int m_CameraRenderPixelSize;

	private int3 m_RtSize;

	private int m_HistoryFramesCount;

	private Matrix4x4 m_PrevViewProjMatrix;

	private bool m_IsFirstFrame;

	private BufferedRTHandleSystem m_HistoryRTSystem = new BufferedRTHandleSystem();

	internal int LastFrameId;

	internal int UnusedFramesCount;

	public Camera Camera => m_Camera;

	public int3 RtSize => m_RtSize;

	public int HistoryFramesCount => m_HistoryFramesCount;

	public Matrix4x4 PrevViewProjMatrix => m_PrevViewProjMatrix;

	public bool IsFirstFrame => m_IsFirstFrame;

	public VolumetricCameraBuffer(Camera camera, Vector2Int cameraRenderPixelSize, int3 rtSize, int historyFramesCount)
	{
		m_Camera = camera;
		m_CameraRenderPixelSize = cameraRenderPixelSize;
		m_RtSize = rtSize;
		m_HistoryFramesCount = historyFramesCount;
		m_IsFirstFrame = true;
		if (m_HistoryFramesCount > 0)
		{
			m_HistoryRTSystem.AllocBuffer(0, AllocVolumetricHistoryBuffer, m_HistoryFramesCount);
		}
	}

	private RTHandle AllocVolumetricHistoryBuffer(RTHandleSystem rts, int frameIndex)
	{
		return rts.Alloc(m_RtSize.x, m_RtSize.y, m_RtSize.z, DepthBits.None, GraphicsFormat.R16G16B16A16_SFloat, FilterMode.Bilinear, TextureWrapMode.Clamp, TextureDimension.Tex3D, enableRandomWrite: true, useMipMap: false, autoGenerateMips: false, isShadowMap: false, 0, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, RenderTextureMemoryless.None, VRTextureUsage.None, $"{m_Camera.name}_VolumetricHistory{frameIndex}");
	}

	public RTHandle GetCurrentFrameRT()
	{
		return m_HistoryRTSystem.GetFrameRT(0, 0);
	}

	public RTHandle GetPreviousFrameRT()
	{
		return m_HistoryRTSystem.GetFrameRT(0, 1);
	}

	public void Swap(Matrix4x4 prevViewProjMatrix)
	{
		m_IsFirstFrame = false;
		m_HistoryRTSystem.SwapAndSetReferenceSize(m_CameraRenderPixelSize.x, m_CameraRenderPixelSize.y);
		m_PrevViewProjMatrix = prevViewProjMatrix;
	}

	public void Dispose()
	{
		m_HistoryRTSystem.Dispose();
	}
}
