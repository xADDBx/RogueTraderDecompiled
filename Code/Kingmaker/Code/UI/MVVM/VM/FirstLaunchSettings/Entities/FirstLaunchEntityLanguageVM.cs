using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings.Entities;

public class FirstLaunchEntityLanguageVM : SettingsEntityDropdownVM
{
	public List<FirstLaunchEntityLanguageItemVM> Items;

	private readonly Action<int> m_ValueSetter;

	public FirstLaunchEntityLanguageVM(UISettingsEntityDropdownLocale uiSettingsEntity, bool forceSetValue = false)
		: base(uiSettingsEntity)
	{
		Items = new List<FirstLaunchEntityLanguageItemVM>();
		for (int i = 0; i < uiSettingsEntity.LocalizedValues.Count; i++)
		{
			string language = uiSettingsEntity.LocalizedValues[i];
			m_ValueSetter = (forceSetValue ? new Action<int>(base.SetValueAndConfirm) : new Action<int>(base.SetTempValue));
			FirstLaunchEntityLanguageItemVM firstLaunchEntityLanguageItemVM = new FirstLaunchEntityLanguageItemVM(language, i, m_ValueSetter, TempIndexValue);
			AddDisposable(firstLaunchEntityLanguageItemVM);
			Items.Add(firstLaunchEntityLanguageItemVM);
		}
	}
}
