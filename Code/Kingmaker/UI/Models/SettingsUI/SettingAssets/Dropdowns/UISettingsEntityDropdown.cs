using System;
using System.Collections.Generic;
using Kingmaker.Settings.Entities;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

public abstract class UISettingsEntityDropdown<TValue> : UISettingsEntityWithValueBase<TValue>, IUISettingsEntityDropdown, IUISettingsEntityWithValueBase, IUISettingsEntityBase
{
	public abstract IReadOnlyList<string> LocalizedValues { get; }

	public override SettingsListItemType? Type => SettingsListItemType.Dropdown;

	public event Action<int> OnTempIndexValueChanged;

	public override void LinkSetting(SettingsEntity<TValue> setting)
	{
		if (setting != Setting)
		{
			base.LinkSetting(setting);
			setting.OnTempValueChanged += delegate
			{
				this.OnTempIndexValueChanged?.Invoke(GetIndexTempValue());
			};
		}
	}

	public abstract int GetIndexTempValue();

	public abstract void SetIndexTempValue(int value);

	public abstract void SetIndexValueAndConfirm(int value);
}
