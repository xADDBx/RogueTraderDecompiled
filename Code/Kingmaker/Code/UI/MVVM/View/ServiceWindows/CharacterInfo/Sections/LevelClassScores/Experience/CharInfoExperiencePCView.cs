using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Templates;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;

public class CharInfoExperiencePCView : CharInfoComponentView<CharInfoExperienceVM>
{
	[Header("Fields")]
	[SerializeField]
	private TextMeshProUGUI m_LevelLabel;

	[SerializeField]
	private TextMeshProUGUI m_Level;

	[Header("Psy Rating")]
	[SerializeField]
	private GameObject m_PsyRatingGroup;

	[SerializeField]
	private Image m_PsyRatingBgr;

	[SerializeField]
	private TextMeshProUGUI m_PsyRatingLabel;

	[SerializeField]
	private TextMeshProUGUI m_PsyRating;

	[Header("Progress Bar")]
	[SerializeField]
	private Image m_ExpRoundImage;

	public override void Initialize()
	{
		base.Initialize();
		m_LevelLabel.text = UIStrings.Instance.CharacterSheet.LvlShort;
		m_PsyRatingLabel.text = UIStrings.Instance.CharacterSheet.PsyRatingShort;
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.Level.Subscribe(delegate(int level)
		{
			m_Level.text = $"{level}";
		}));
		AddDisposable(base.ViewModel.HasPsyRating.Subscribe(m_PsyRatingGroup.SetActive));
		AddDisposable(base.ViewModel.PsyRating.Subscribe(delegate(int psyRating)
		{
			m_PsyRating.text = $"{psyRating}";
		}));
		if (m_ExpRoundImage != null)
		{
			AddDisposable(base.ViewModel.CurrentLevelExpRatio.Subscribe(delegate(float value)
			{
				m_ExpRoundImage.fillAmount = value;
			}));
		}
	}

	protected override void RefreshView()
	{
		SetTooltips();
	}

	private void SetTooltips()
	{
		AddDisposable(this.SetTooltip(new TooltipTemplateLevelExp(base.ViewModel)));
		if ((bool)m_PsyRatingBgr)
		{
			AddDisposable(m_PsyRatingBgr.SetTooltip(base.ViewModel.PsyRatingTooltip));
		}
	}
}
