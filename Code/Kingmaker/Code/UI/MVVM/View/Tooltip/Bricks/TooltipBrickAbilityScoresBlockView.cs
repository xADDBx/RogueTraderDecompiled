using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Bricks;
using Kingmaker.UI.Common;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.Tooltip.Bricks;

public class TooltipBrickAbilityScoresBlockView : TooltipBaseBrickView<TooltipBrickAbilityScoresBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Title;

	[SerializeField]
	protected CharInfoAbilityScoresBlockBaseView m_AbilityScoresBlockView;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title);
		}
		base.BindViewImplementation();
		m_Title.text = UIStrings.Instance.Inspect.CharacterStatsTitle.Text;
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_TextHelper.Dispose();
	}
}
