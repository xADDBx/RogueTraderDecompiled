using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.UI.MVVM;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.Careers.Common;

public class RankExpCounterCommonView : ViewBase<CharInfoExperienceVM>
{
	[SerializeField]
	private RectTransform m_LayoutRect;

	[Header("Ranks")]
	[SerializeField]
	private bool n_HasNewRanks = true;

	[SerializeField]
	[ConditionalShow("n_HasNewRanks")]
	private GameObject m_RanksContainer;

	[SerializeField]
	[ConditionalShow("n_HasNewRanks")]
	private TextMeshProUGUI m_RanksDesc;

	[SerializeField]
	[ConditionalShow("n_HasNewRanks")]
	private TextMeshProUGUI m_RanksCount;

	[Header("Exp")]
	[SerializeField]
	private bool n_HasExp = true;

	[SerializeField]
	[ConditionalShow("n_HasExp")]
	private TextMeshProUGUI m_ExpLabel;

	[SerializeField]
	[ConditionalShow("n_HasExp")]
	private Image m_ExpProgressBar;

	[Header("Level")]
	[SerializeField]
	private bool n_HasLevel = true;

	[SerializeField]
	[ConditionalShow("n_HasLevel")]
	private TextMeshProUGUI m_CurrentLevelLabel;

	private AccessibilityTextHelper m_TextHelper;

	protected override void BindViewImplementation()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_RanksDesc, m_RanksCount, m_ExpLabel, m_CurrentLevelLabel);
		}
		if (n_HasNewRanks)
		{
			AddDisposable(base.ViewModel.NewRanksCount.Subscribe(delegate(int ranks)
			{
				m_RanksContainer.SetActive(ranks > 0);
				m_RanksCount.text = ranks.ToString();
			}));
			AddDisposable(this.SetHint(UIStrings.Instance.CharacterSheet.AvailableRanksHint));
		}
		if (n_HasExp)
		{
			AddDisposable(base.ViewModel.CurrentExp.CombineLatest(base.ViewModel.NextLevelExp, (int currentExp, int nextExp) => new { currentExp, nextExp }).Subscribe(value =>
			{
				m_ExpLabel.text = $"{value.currentExp}/{value.nextExp}";
			}));
			AddDisposable(base.ViewModel.CurrentLevelExpRatio.Subscribe(delegate(float value)
			{
				m_ExpProgressBar.fillAmount = value;
			}));
		}
		if (n_HasLevel)
		{
			AddDisposable(base.ViewModel.Level.Subscribe(delegate(int level)
			{
				m_CurrentLevelLabel.text = string.Format(UIStrings.Instance.CharacterSheet.CurrentLevelLabel.Text, level);
				LayoutRebuilder.ForceRebuildLayoutImmediate(m_LayoutRect);
			}));
		}
		m_RanksDesc.text = string.Format(UIStrings.Instance.CharacterSheet.RanksCounterLabel.Text, "");
		m_TextHelper.UpdateTextSize();
	}

	protected override void DestroyViewImplementation()
	{
		m_TextHelper.Dispose();
	}
}
