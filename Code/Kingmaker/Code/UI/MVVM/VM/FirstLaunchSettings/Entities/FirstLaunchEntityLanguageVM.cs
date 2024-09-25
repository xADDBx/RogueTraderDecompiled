using System;
using System.Collections.Generic;
using Kingmaker.Code.UI.MVVM.VM.Settings.Entities;
using Kingmaker.Localization.Enums;
using Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

namespace Kingmaker.Code.UI.MVVM.VM.FirstLaunchSettings.Entities;

public class FirstLaunchEntityLanguageVM : SettingsEntityDropdownVM
{
	public readonly List<FirstLaunchEntityLanguageItemVM> Items;

	public FirstLaunchEntityLanguageVM(UISettingsEntityDropdownLocale uiSettingsEntity, bool forceSetValue = false)
		: base(uiSettingsEntity)
	{
		Items = new List<FirstLaunchEntityLanguageItemVM>();
		for (int i = 0; i < uiSettingsEntity.LocalizedValues.Count; i++)
		{
			if (!(uiSettingsEntity.LocalizedValues[i] == Locale.dev.ToString()))
			{
				string language = uiSettingsEntity.LocalizedValues[i];
				Action<int> setSelected = (forceSetValue ? new Action<int>(base.SetValueAndConfirm) : new Action<int>(base.SetTempValue));
				FirstLaunchEntityLanguageItemVM firstLaunchEntityLanguageItemVM = new FirstLaunchEntityLanguageItemVM(language, i, setSelected, TempIndexValue);
				AddDisposable(firstLaunchEntityLanguageItemVM);
				Items.Add(firstLaunchEntityLanguageItemVM);
			}
		}
	}
}
