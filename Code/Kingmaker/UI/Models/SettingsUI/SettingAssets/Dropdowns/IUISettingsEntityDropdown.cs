using System;
using System.Collections.Generic;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

public interface IUISettingsEntityDropdown : IUISettingsEntityWithValueBase, IUISettingsEntityBase
{
	IReadOnlyList<string> LocalizedValues { get; }

	event Action<int> OnTempIndexValueChanged;

	int GetIndexTempValue();

	void SetIndexTempValue(int value);

	void SetIndexValueAndConfirm(int value);
}
