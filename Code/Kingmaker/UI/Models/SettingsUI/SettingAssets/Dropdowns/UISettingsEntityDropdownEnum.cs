using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

public class UISettingsEntityDropdownEnum<TEnum> : UISettingsEntityDropdown<TEnum> where TEnum : Enum
{
	public IReadOnlyList<string> m_CashedLocalizedValues;

	protected Locale m_CachedLocale;

	[SerializeField]
	protected LocalizedString[] values = Array.Empty<LocalizedString>();

	public override IReadOnlyList<string> LocalizedValues
	{
		get
		{
			bool flag = m_CachedLocale != LocalizationManager.Instance.CurrentLocale;
			if (m_CashedLocalizedValues == null || m_CashedLocalizedValues.Count == 0 || flag)
			{
				m_CashedLocalizedValues = ((IEnumerable<LocalizedString>)values).Select((Func<LocalizedString, string>)((LocalizedString v) => v)).ToArray();
				m_CachedLocale = LocalizationManager.Instance.CurrentLocale;
			}
			return m_CashedLocalizedValues;
		}
	}

	public override SettingsListItemType? Type => SettingsListItemType.Dropdown;

	public override int GetIndexTempValue()
	{
		return EnumUtils.GetOrdinalNumber(GetTempValue());
	}

	public override void SetIndexTempValue(int value)
	{
		SetTempValue(EnumUtils.GetValueInOrder<TEnum>(value));
	}

	public override void SetIndexValueAndConfirm(int value)
	{
		SetValueAndConfirm(EnumUtils.GetValueInOrder<TEnum>(value));
	}
}
