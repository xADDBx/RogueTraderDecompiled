using System;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual.Particles;

[RequireComponent(typeof(Camera))]
public class LowResParticlesRenderer : MonoBehaviour
{
	public enum Downscale
	{
		Downscale2x2 = 2,
		Downscale4x4 = 4,
		Downscale16x16Debug = 0x10
	}

	public LayerMask CullingMask;

	public Downscale DownscaleValue = Downscale.Downscale2x2;

	public Shader LowResParticlesShader;

	private Camera m_Camera;

	private Material m_Material;

	private void OnEnable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Combine(Camera.onPostRender, new Camera.CameraCallback(OnCameraPostRender));
		m_Camera = GetComponent<Camera>();
		m_Material = new Material(LowResParticlesShader);
	}

	private void OnDisable()
	{
		Camera.onPostRender = (Camera.CameraCallback)Delegate.Remove(Camera.onPostRender, new Camera.CameraCallback(OnCameraPostRender));
		if (Game.GetCamera() != null)
		{
			Game.GetCamera().cullingMask |= CullingMask;
		}
	}

	private Vector2 GetRTSize()
	{
		float num = 1f / (float)DownscaleValue;
		Vector2 result = new Vector2((float)Screen.width * num, (float)Screen.height * num);
		if (Game.GetCamera() != null && Game.GetCamera().targetTexture != null)
		{
			result = new Vector2((float)Game.GetCamera().targetTexture.width * num, (float)Game.GetCamera().targetTexture.height * num);
		}
		return result;
	}

	private void OnCameraPostRender(Camera cam)
	{
		if (!(cam != Game.GetCamera()))
		{
			cam.cullingMask &= ~(int)CullingMask;
			Vector2 rTSize = GetRTSize();
			RenderTexture temporary = RenderTexture.GetTemporary((int)rTSize.x, (int)rTSize.y, 24, RenderTextureFormat.RFloat);
			RenderTexture temporary2 = RenderTexture.GetTemporary(temporary.width, temporary.height, 0, RenderTextureFormat.ARGB32);
			Graphics.SetRenderTarget(temporary);
			Graphics.Blit(null, m_Material, 0);
			Graphics.SetRenderTarget(temporary2);
			GL.Clear(clearDepth: false, clearColor: true, new Color(0f, 0f, 0f, 1f));
			m_Camera.enabled = false;
			m_Camera.cullingMask = CullingMask;
			m_Camera.clearFlags = CameraClearFlags.Nothing;
			m_Camera.depthTextureMode = DepthTextureMode.None;
			m_Camera.aspect = cam.aspect;
			m_Camera.farClipPlane = cam.farClipPlane;
			m_Camera.fieldOfView = cam.fieldOfView;
			m_Camera.nearClipPlane = cam.nearClipPlane;
			m_Camera.orthographic = cam.orthographic;
			m_Camera.orthographicSize = cam.orthographicSize;
			m_Camera.transform.position = cam.transform.position;
			m_Camera.transform.rotation = cam.transform.rotation;
			m_Camera.SetTargetBuffers(temporary2.colorBuffer, temporary.depthBuffer);
			m_Camera.Render();
			Graphics.SetRenderTarget(cam.targetTexture);
			RenderTexture temporary3 = RenderTexture.GetTemporary(temporary2.width, temporary2.height, 0, temporary2.format);
			Graphics.Blit(temporary2, temporary3);
			temporary.filterMode = FilterMode.Point;
			temporary2.filterMode = FilterMode.Bilinear;
			temporary3.filterMode = FilterMode.Point;
			m_Material.SetTexture(ShaderProps._LowResColorBilinear, temporary2);
			m_Material.SetTexture(ShaderProps._LowResColorPoint, temporary3);
			m_Material.SetTexture(ShaderProps._LowResDepth, temporary);
			Graphics.Blit(null, cam.targetTexture, m_Material, 1);
			RenderTexture.ReleaseTemporary(temporary);
			RenderTexture.ReleaseTemporary(temporary2);
			RenderTexture.ReleaseTemporary(temporary3);
		}
	}
}
