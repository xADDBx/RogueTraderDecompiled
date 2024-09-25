using Kingmaker.Visual.Decals;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.UI.Pointer;

public class ClickPointerPrefab : MonoBehaviour
{
	public GUIDecal[] Decals;

	public bool IsVisible;

	private float m_CurrentTransparency = -1f;

	private bool m_NeedUpdate;

	public void SetVisible(bool visible, float f = 1f)
	{
		if (IsVisible == visible && m_CurrentTransparency == f)
		{
			return;
		}
		IsVisible = visible;
		m_CurrentTransparency = f;
		base.gameObject.SetActive(IsVisible);
		if (IsVisible)
		{
			GUIDecal[] decals = Decals;
			foreach (GUIDecal obj in decals)
			{
				Color white = Color.white;
				white.a = 0f;
				obj.MaterialProperties.SetColor(ShaderProps._Color, white);
			}
			m_NeedUpdate = true;
		}
	}

	public void Update()
	{
		if (!m_NeedUpdate)
		{
			return;
		}
		bool flag = false;
		GUIDecal[] decals = Decals;
		for (int i = 0; i < decals.Length; i++)
		{
			MaterialPropertyBlock materialProperties = decals[i].MaterialProperties;
			Color color = materialProperties.GetColor(ShaderProps._Color);
			float num = color.a + m_CurrentTransparency * (Time.fixedUnscaledDeltaTime / 0.1f);
			if (num >= m_CurrentTransparency)
			{
				num = m_CurrentTransparency;
				flag = true;
			}
			color.a = num;
			materialProperties.SetColor(ShaderProps._Color, color);
		}
		if (flag)
		{
			m_NeedUpdate = false;
		}
	}
}
