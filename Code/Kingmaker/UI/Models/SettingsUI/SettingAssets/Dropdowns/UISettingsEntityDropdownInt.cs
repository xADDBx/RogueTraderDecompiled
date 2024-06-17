using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

[CreateAssetMenu(menuName = "Settings UI/Dropdown Int")]
public class UISettingsEntityDropdownInt : UISettingsEntityDropdown<int>
{
	public List<string> m_LocalizedValues;

	public override IReadOnlyList<string> LocalizedValues => m_LocalizedValues;

	public void SetLocalizedValues(List<string> localizedValues)
	{
		m_LocalizedValues = localizedValues;
	}

	public override int GetIndexTempValue()
	{
		return GetTempValue();
	}

	public override void SetIndexTempValue(int value)
	{
		SetTempValue(value);
	}

	public override void SetIndexValueAndConfirm(int value)
	{
		SetValueAndConfirm(value);
	}
}
