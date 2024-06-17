using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Settings.Difficulty;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.Code.UI.MVVM.VM.Settings.Entities.Difficulty;

public class SettingsEntityDropdownGameDifficultyVM : SettingsEntityDropdownVM
{
	public readonly List<SettingsEntityDropdownGameDifficultyItemVM> Items;

	private readonly Action<int> m_ValueSetter;

	public SettingsEntityDropdownGameDifficultyVM(UISettingsEntityGameDifficulty uiSettingsEntity, bool forceSetValue = false, bool hideMarkImage = false)
		: base(uiSettingsEntity, DropdownType.Default, hideMarkImage)
	{
		Items = new List<SettingsEntityDropdownGameDifficultyItemVM>();
		for (int i = 0; i < BlueprintRoot.Instance.DifficultyList.Difficulties.Count; i++)
		{
			DifficultyPresetAsset difficulty = BlueprintRoot.Instance.DifficultyList.Difficulties[i];
			m_ValueSetter = (forceSetValue ? new Action<int>(base.SetValueAndConfirm) : new Action<int>(base.SetTempValue));
			SettingsEntityDropdownGameDifficultyItemVM settingsEntityDropdownGameDifficultyItemVM = new SettingsEntityDropdownGameDifficultyItemVM(difficulty, i, m_ValueSetter, TempIndexValue);
			AddDisposable(settingsEntityDropdownGameDifficultyItemVM);
			Items.Add(settingsEntityDropdownGameDifficultyItemVM);
		}
	}

	public void SetValue(int index)
	{
		m_ValueSetter?.Invoke(index);
	}
}
