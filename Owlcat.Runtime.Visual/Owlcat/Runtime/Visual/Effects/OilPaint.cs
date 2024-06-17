using UnityEngine;

namespace Owlcat.Runtime.Visual.Effects;

public class OilPaint : MonoBehaviour
{
	[SerializeField]
	private Shader m_OilPaintShader;

	[SerializeField]
	private float m_Radius = 2f;

	[SerializeField]
	private float m_ResolutionValue = 1f;

	private Material m_OilPaintMaterial;

	private RenderTexture m_Rt;

	private Texture m_OriginalTexture;

	private Material m_OriginalMaterial;

	private void OnEnable()
	{
		if (m_OilPaintShader == null)
		{
			return;
		}
		Renderer component = GetComponent<Renderer>();
		if (!(component != null))
		{
			return;
		}
		m_OriginalMaterial = component.sharedMaterial;
		m_OriginalTexture = (m_OriginalMaterial.HasProperty("_BaseMap") ? m_OriginalMaterial.GetTexture("_BaseMap") : null);
		if (m_OriginalTexture != null)
		{
			m_OriginalTexture.wrapMode = TextureWrapMode.Clamp;
			m_Rt = RenderTexture.GetTemporary(m_OriginalTexture.width, m_OriginalTexture.height, 0, RenderTextureFormat.ARGB32);
			m_Rt.name = m_OriginalTexture.name + "_OilPaintRT";
			if (m_OilPaintMaterial == null)
			{
				m_OilPaintMaterial = new Material(m_OilPaintShader);
			}
			m_OilPaintMaterial.SetFloat("_Radius", m_Radius);
			m_OilPaintMaterial.SetFloat("_ResolutionValue", m_ResolutionValue);
			m_OilPaintMaterial.SetFloat("_Width", m_OriginalTexture.width);
			m_OilPaintMaterial.SetFloat("_Height", m_OriginalTexture.height);
			Graphics.Blit(m_OriginalTexture, m_Rt, m_OilPaintMaterial, 0);
			m_OriginalMaterial.SetTexture("_BaseMap", m_Rt);
		}
	}

	private void OnDisable()
	{
		if (m_Rt != null)
		{
			RenderTexture.ReleaseTemporary(m_Rt);
			m_Rt = null;
		}
		if (m_OriginalMaterial != null && m_OriginalTexture != null)
		{
			m_OriginalMaterial.SetTexture("_BaseMap", m_OriginalTexture);
		}
	}
}
