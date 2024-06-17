using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

[RequireComponent(typeof(Camera))]
public class WaaaghCameraScreenshoter : MonoBehaviour
{
	private enum CaptureState
	{
		None,
		InProcess,
		Executed
	}

	private enum CaptureType
	{
		Texture,
		Png
	}

	private Camera m_Camera;

	private Action<RenderTargetIdentifier, CommandBuffer> m_CaptureAction;

	private Action<Texture2D> m_ResultCallback;

	private Action<byte[]> m_PngCallback;

	private Texture2D m_ResultTexture;

	private RenderTexture m_RenderTexture;

	private CaptureState m_CaptureState;

	private CaptureType m_CaptureType;

	private void OnEnable()
	{
		m_Camera = GetComponent<Camera>();
		m_CaptureAction = OnCameraCapture;
	}

	private void OnDisable()
	{
		ReleaseRt();
	}

	private void ReleaseRt()
	{
		if (m_RenderTexture != null)
		{
			m_RenderTexture.Release();
			m_RenderTexture = null;
		}
	}

	private void Update()
	{
		switch (m_CaptureState)
		{
		case CaptureState.Executed:
		{
			CameraCaptureBridge.RemoveCaptureAction(m_Camera, m_CaptureAction);
			m_CaptureState = CaptureState.None;
			RenderTexture active = RenderTexture.active;
			RenderTexture.active = m_RenderTexture;
			m_ResultTexture.ReadPixels(new Rect(0f, 0f, m_ResultTexture.width, m_ResultTexture.height), 0, 0);
			m_ResultTexture.Apply();
			RenderTexture.active = active;
			ReleaseRt();
			switch (m_CaptureType)
			{
			case CaptureType.Texture:
				m_ResultCallback(m_ResultTexture);
				break;
			case CaptureType.Png:
				m_PngCallback(m_ResultTexture.EncodeToPNG());
				UnityEngine.Object.DestroyImmediate(m_ResultTexture);
				break;
			}
			break;
		}
		case CaptureState.None:
		case CaptureState.InProcess:
			break;
		}
	}

	private void OnCameraCapture(RenderTargetIdentifier cameraColorBuffer, CommandBuffer cmd)
	{
		if (m_RenderTexture == null)
		{
			m_RenderTexture = new RenderTexture(m_ResultTexture.width, m_ResultTexture.height, 0, RenderTextureFormat.ARGBHalf);
		}
		cmd.Blit(cameraColorBuffer, m_RenderTexture);
		m_CaptureState = CaptureState.Executed;
	}

	public void MakeScreenshot(Texture2D result, Action<Texture2D> callback)
	{
		if (result == null)
		{
			throw new NullReferenceException("Result texture is null");
		}
		if (callback == null)
		{
			throw new NullReferenceException("Callback is null");
		}
		m_ResultTexture = result;
		m_ResultCallback = callback;
		m_CaptureType = CaptureType.Texture;
		m_CaptureState = CaptureState.InProcess;
		CameraCaptureBridge.AddCaptureAction(m_Camera, m_CaptureAction);
	}

	public void MakePNG(int width, int height, Action<byte[]> callback)
	{
		if (callback == null)
		{
			throw new NullReferenceException("Callback is null");
		}
		width = ((width == 0) ? Screen.width : width);
		height = ((height == 0) ? Screen.height : height);
		m_ResultTexture = new Texture2D(width, height, TextureFormat.ARGB32, mipChain: false);
		m_PngCallback = callback;
		m_CaptureType = CaptureType.Png;
		m_CaptureState = CaptureState.InProcess;
		CameraCaptureBridge.AddCaptureAction(m_Camera, m_CaptureAction);
	}
}
