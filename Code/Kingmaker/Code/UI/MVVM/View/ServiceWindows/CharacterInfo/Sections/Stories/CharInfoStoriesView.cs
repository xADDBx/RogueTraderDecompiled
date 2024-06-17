using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.Stories;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.Tooltips;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Stories;

public class CharInfoStoriesView : CharInfoComponentView<CharInfoStoriesVM>
{
	[SerializeField]
	private TextMeshProUGUI m_BiographyTitle;

	[SerializeField]
	private TextMeshProUGUI m_BiographyText;

	[SerializeField]
	private TextMeshProUGUI m_EmptyBiographyText;

	[SerializeField]
	private GameObject m_EmptyBiographyGroup;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_BiographyTitle, m_BiographyText, m_EmptyBiographyText);
		}
		base.BindViewImplementation();
		AddDisposable(m_BiographyText.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: true)));
		m_BiographyTitle.text = UIStrings.Instance.CharacterSheet.Biography;
		m_EmptyBiographyText.text = UIStrings.Instance.CharacterSheet.EmptyBiographyDesc;
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		bool flag = base.ViewModel.Stories.Count > 0;
		m_EmptyBiographyGroup.SetActive(!flag);
		string text = (flag ? base.ViewModel.Stories[0].StoryText : "");
		m_BiographyText.text = text;
	}
}
