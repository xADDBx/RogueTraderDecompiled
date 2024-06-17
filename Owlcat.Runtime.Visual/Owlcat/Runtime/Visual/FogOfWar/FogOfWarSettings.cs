using UnityEngine;

namespace Owlcat.Runtime.Visual.FogOfWar;

[CreateAssetMenu(menuName = "Owlcat/Fow Of War Settings")]
public class FogOfWarSettings : ScriptableObject
{
	[SerializeField]
	private Color m_Color = new Color(0f, 0f, 0f, 0.5f);

	[SerializeField]
	private float m_ShadowFalloff = 0.15f;

	[SerializeField]
	private float m_ShadowCullingHeightOffset = 1f;

	[SerializeField]
	private float m_ShadowCullingHeight = 1f;

	[SerializeField]
	private float m_RevealerInnerRadius = 1f;

	[SerializeField]
	private float m_RevealerOutterRadius = 11.7f;

	[SerializeField]
	private float m_BorderWidth = 6.35f;

	[SerializeField]
	private float m_BorderOffset = 3.22f;

	[SerializeField]
	private bool m_IsBlurEnabled = true;

	[SerializeField]
	[Range(1f, 4f)]
	private int m_BlurIterations = 2;

	[SerializeField]
	[Range(0f, 10f)]
	private float m_BlurSize = 3f;

	[Range(0f, 2f)]
	private int m_BlurDownsample = 1;

	[SerializeField]
	private BlurType m_BlurType;

	[SerializeField]
	private float m_TextureDensity = 5f;

	private static FogOfWarSettings s_Instance;

	public Color Color => m_Color;

	public float ShadowFalloff => m_ShadowFalloff;

	public float ShadowCullingHeightOffset => m_ShadowCullingHeightOffset;

	public float ShadowCullingHeight => m_ShadowCullingHeight;

	public float RevealerInnerRadius => m_RevealerInnerRadius;

	public float RevealerOutterRadius => m_RevealerOutterRadius;

	public float BorderWidth => m_BorderWidth;

	public float BorderOffset => m_BorderOffset;

	public bool IsBlurEnabled => m_IsBlurEnabled;

	public int BlurIterations => m_BlurIterations;

	public float BlurSize => m_BlurSize;

	public int BlurDownsample => m_BlurDownsample;

	public BlurType BlurType => m_BlurType;

	public float TextureDensity => m_TextureDensity;

	public static FogOfWarSettings Instance => s_Instance;

	internal void InitSingleton(FogOfWarSettings instance)
	{
		s_Instance = instance;
	}
}
