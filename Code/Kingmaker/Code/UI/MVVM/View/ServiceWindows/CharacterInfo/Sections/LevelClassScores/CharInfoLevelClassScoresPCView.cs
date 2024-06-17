using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.AbilityScores;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Classes;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores.Experience;
using Kingmaker.Code.UI.MVVM.View.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CharacterInfo.Sections.LevelClassScores;
using Owlcat.Runtime.Core.Utility;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View.ServiceWindows.CharacterInfo.Sections.LevelClassScores;

public class CharInfoLevelClassScoresPCView : CharInfoComponentView<CharInfoLevelClassScoresVM>
{
	[SerializeField]
	private CharInfoExperiencePCView m_Experience;

	[SerializeField]
	private CharInfoAbilityScoresBlockBaseView m_AbilityScores;

	[SerializeField]
	private CharInfoClassesListPCView m_Classes;

	[Header("Add stats")]
	[SerializeField]
	protected InventoryDollAdditionalStatsPCView m_AdditionalStatsView;

	[Header("Localization")]
	[SerializeField]
	private TextMeshProUGUI m_CharacterStatsLabel;

	public override void Initialize()
	{
		base.Initialize();
		m_Experience.Or(null)?.Initialize();
		m_AbilityScores.Or(null)?.Initialize();
		m_Classes.Or(null)?.Initialize();
		m_AdditionalStatsView.Or(null)?.Initialize();
	}

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		m_Experience.Or(null)?.Bind(base.ViewModel.Experience);
		m_AdditionalStatsView.Or(null)?.Bind(base.ViewModel.AdditionalStatsVM);
		m_CharacterStatsLabel.text = UIStrings.Instance.CharacterSheet.Stats;
	}

	protected override void RefreshView()
	{
		m_AbilityScores.Or(null)?.Bind(base.ViewModel.AbilityScores);
		m_Classes.Or(null)?.Bind(base.ViewModel.Classes);
	}
}
