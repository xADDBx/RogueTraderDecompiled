using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;

public class CharInfoAvailableRanksCommonView : ViewBase<CharInfoAvailableRanksVM>
{
	[SerializeField]
	private GameObject m_RanksContainer;

	[SerializeField]
	private TextMeshProUGUI m_RanksDesc;

	[SerializeField]
	private TextMeshProUGUI m_RanksCount;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RanksCount, m_RanksDesc);
		}
		AddDisposable(base.ViewModel.NewRanksCount.Subscribe(delegate(int ranks)
		{
			m_RanksContainer.SetActive(ranks > 0 && base.ViewModel.IsInLevelupProcess);
			m_RanksCount.text = ranks.ToString();
		}));
		AddDisposable(this.SetHint(UIStrings.Instance.CharacterSheet.AvailableRanksHint));
		m_RanksDesc.text = string.Format(UIStrings.Instance.CharacterSheet.RanksCounterLabel.Text, "");
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}
}
