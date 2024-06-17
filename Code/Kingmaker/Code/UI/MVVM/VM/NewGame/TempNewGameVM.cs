using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;
using Kingmaker.Settings;
using Kingmaker.UI.Models.LevelUp;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;
using Kingmaker.UI.MVVM.VM.CharGen.Phases.Pregen;
using Owlcat.Runtime.UI.SelectionGroup;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.NewGame;

public class TempNewGameVM : NewGamePhaseBaseVm
{
	private readonly List<CharGenPregenSelectorItemVM> m_PregenEntitiesList = new List<CharGenPregenSelectorItemVM>();

	private readonly ReactiveProperty<CharGenPregenSelectorItemVM> m_SelectedPregenEntity = new ReactiveProperty<CharGenPregenSelectorItemVM>();

	public readonly SettingsEntityDropdownGameDifficultyVM SettingsEntityDropdownGameDifficultyVM;

	public SelectionGroupRadioVM<CharGenPregenSelectorItemVM> PregenSelectionGroupVM { get; }

	public TempNewGameVM(Action backStep, Action nextStep)
		: base(backStep, nextStep)
	{
		SettingsRoot.Difficulty.GameDifficulty.SetValueAndConfirm(GameDifficultyOption.Normal);
		List<ChargenUnit> pregensForChargen = BlueprintRoot.Instance.CharGenRoot.GetPregensForChargen();
		for (int i = 0; i < pregensForChargen.Count; i++)
		{
			ChargenUnit chargenUnit = pregensForChargen[i];
			m_PregenEntitiesList.Add(new CharGenPregenSelectorItemVM(chargenUnit));
		}
		AddDisposable(PregenSelectionGroupVM = new SelectionGroupRadioVM<CharGenPregenSelectorItemVM>(m_PregenEntitiesList, m_SelectedPregenEntity));
		m_SelectedPregenEntity.Value = m_PregenEntitiesList.First();
		AddDisposable(m_SelectedPregenEntity.Subscribe(SetPregen));
		UISettingsEntityGameDifficulty gameDifficulty = BlueprintRoot.Instance.UISettingsRoot.UISettings.UIDifficultySettings.GameDifficulty;
		AddDisposable(SettingsEntityDropdownGameDifficultyVM = new SettingsEntityDropdownGameDifficultyVM(gameDifficulty, forceSetValue: true));
	}

	protected override void DisposeImplementation()
	{
		m_PregenEntitiesList.ForEach(delegate(CharGenPregenSelectorItemVM item)
		{
			item.Dispose();
		});
		m_PregenEntitiesList.Clear();
	}

	private void SetPregen(CharGenPregenSelectorItemVM pregen)
	{
		Game.NewGameUnit = pregen.ChargenUnit.Unit;
	}
}
