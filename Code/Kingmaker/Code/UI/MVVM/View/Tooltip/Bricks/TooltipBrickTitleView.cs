using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickTitleView : TooltipBaseBrickView<TooltipBrickTitleVM>
{
	[SerializeField]
	protected List<GameObject> m_TitleObjects;

	[SerializeField]
	private List<TextMeshProUGUI> m_Titles;

	[SerializeField]
	private List<LayoutGroup> m_LayoutGroups;

	[SerializeField]
	private List<float> m_DefaultFontSizes = new List<float> { 27f, 25f, 22f, 18f, 14f, 16f };

	[SerializeField]
	private List<float> m_DefaultConsoleFontSizes = new List<float> { 27f, 25f, 22f, 18f, 14f, 16f };

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Titles.Cast<TMP_Text>().ToArray());
		}
		base.BindViewImplementation();
		int type = (int)base.ViewModel.Type;
		for (int i = 0; i < m_TitleObjects.Count; i++)
		{
			m_TitleObjects[i].SetActive(type == i);
		}
		m_TextHelper.UpdateTextSize();
		if (m_Titles.Count > type)
		{
			TextMeshProUGUI textMeshProUGUI = m_Titles[type];
			textMeshProUGUI.text = base.ViewModel.Title;
			textMeshProUGUI.alignment = base.ViewModel.Alignment;
			textMeshProUGUI.fontSize += base.ViewModel.AdditionalTextSize;
			if (m_LayoutGroups != null && m_LayoutGroups.Count > type && m_LayoutGroups[type] != null)
			{
				m_LayoutGroups[type].childAlignment = base.ViewModel.TextAnchor;
			}
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
