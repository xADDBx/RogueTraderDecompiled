using Kingmaker.Settings;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Visual;

[ImageEffectAllowedInSceneView]
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
[ExecuteInEditMode]
public class GammaAdjustment : MonoBehaviour
{
	private const float MIN_GAMMA = 0.3f;

	private const float MAX_GAMMA = 4f;

	private Material m_Material;

	[Range(0f, 4f)]
	[SerializeField]
	private float m_Gamma;

	private static int s_LastApplyFrame = -1;

	private void Start()
	{
		m_Material = new Material(Shader.Find("Hidden/GammaAdjustment"));
		Init();
	}

	public void Init()
	{
		if (true)
		{
			SettingsRoot.Display.GammaCorrection.OnValueChanged += SetGamma01;
			SetGamma01(SettingsRoot.Display.GammaCorrection);
		}
		else
		{
			SetGamma01(SettingsRoot.Display.GammaCorrection.DefaultValue);
		}
	}

	public static float GetDefaultGamma01()
	{
		return 0.8108108f;
	}

	public void SetGamma01(float gamma)
	{
		gamma = Mathf.Clamp01(gamma);
		m_Gamma = Mathf.Lerp(4f, 0.3f, gamma);
	}

	private void OnRenderImage(RenderTexture source, RenderTexture destination)
	{
		if (Application.isPlaying && Time.frameCount == s_LastApplyFrame)
		{
			PFLog.Default.Warning(this, $"Applying gamma adjustment several times. Current: {this}");
			m_Material.SetFloat(ShaderProps._Gamma, 0.3f);
		}
		else if (Application.isPlaying)
		{
			m_Material.SetFloat(ShaderProps._Gamma, m_Gamma);
		}
		else
		{
			m_Material.SetFloat(ShaderProps._Gamma, SettingsRoot.Display.GammaCorrection.DefaultValue);
		}
		s_LastApplyFrame = Time.frameCount;
		Graphics.Blit(source, destination, m_Material, 0);
	}
}
