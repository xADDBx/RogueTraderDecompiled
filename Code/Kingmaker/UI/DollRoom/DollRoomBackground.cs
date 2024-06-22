using Kingmaker.Visual;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Kingmaker.UI.DollRoom;

public class DollRoomBackground : MonoBehaviour
{
	private Material m_Material;

	private Material m_BlurMaterial;

	public int2 Resolution = new int2(512, 512);

	public bool BlurEnabled;

	[Range(1f, 4f)]
	public int BlurIterations = 2;

	public BlurType BlurType;

	[Range(0f, 10f)]
	public float BlurSize = 3f;

	private RenderTexture m_BackgroundRt;

	private void OnEnable()
	{
		EnsureBlurMaterial();
	}

	public void EnsureBlurMaterial()
	{
		if (m_BlurMaterial == null)
		{
			m_BlurMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/MobileBlur"));
		}
		PFLog.UI.Log("MAT = " + m_BlurMaterial);
	}

	private void OnDisable()
	{
		ReleaseTexture();
		CoreUtils.Destroy(m_BlurMaterial);
	}

	internal RenderTexture Render(ShaderPropertyDescriptor backgroundDollPostProcessProperty)
	{
		EnsureBlurMaterial();
		EnsureRenderTexture();
		CameraStackScreenshoter.TakeScreenshot(m_BackgroundRt);
		if (BlurEnabled)
		{
			DoBlur();
		}
		backgroundDollPostProcessProperty.TextureValue = m_BackgroundRt;
		return m_BackgroundRt;
	}

	private void DoBlur()
	{
		int num = ((BlurType != 0) ? 2 : 0);
		RenderTexture temporary = RenderTexture.GetTemporary(m_BackgroundRt.width, m_BackgroundRt.height, 0, m_BackgroundRt.format);
		for (int i = 0; i < BlurIterations; i++)
		{
			float num2 = (float)i * 1f;
			Shader.SetGlobalVector(FogOfWarConstantBuffer._Parameter, new Vector4(BlurSize + num2, 0f - BlurSize - num2, 0f, 0f));
			Graphics.Blit(m_BackgroundRt, temporary, m_BlurMaterial, 1 + num);
			Graphics.Blit(temporary, m_BackgroundRt, m_BlurMaterial, 2 + num);
		}
		RenderTexture.ReleaseTemporary(temporary);
	}

	private void EnsureRenderTexture()
	{
		if (m_BackgroundRt == null || m_BackgroundRt.width != Resolution.x || m_BackgroundRt.height != Resolution.y)
		{
			ReleaseTexture();
			m_BackgroundRt = new RenderTexture(Resolution.x, Resolution.y, 0, RenderTextureFormat.ARGBHalf);
		}
	}

	private void ReleaseTexture()
	{
		if (m_BackgroundRt != null)
		{
			m_BackgroundRt.Release();
			m_BackgroundRt = null;
		}
	}
}
