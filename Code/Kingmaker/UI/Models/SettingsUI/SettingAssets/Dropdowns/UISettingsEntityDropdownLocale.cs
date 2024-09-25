using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Localization;
using Kingmaker.Localization.Enums;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets.Dropdowns;

[CreateAssetMenu(menuName = "Settings/Main/Locale")]
public class UISettingsEntityDropdownLocale : UISettingsEntityDropdownEnum<Locale>
{
	private static Dictionary<int, Locale> m_IndexToLocaleMap = new Dictionary<int, Locale>
	{
		{
			0,
			Locale.enGB
		},
		{
			1,
			Locale.ruRU
		},
		{
			2,
			Locale.deDE
		},
		{
			3,
			Locale.frFR
		},
		{
			4,
			Locale.zhCN
		},
		{
			5,
			Locale.esES
		},
		{
			6,
			Locale.jaJP
		},
		{
			7,
			Locale.dev
		}
	};

	public override IReadOnlyList<string> LocalizedValues
	{
		get
		{
			bool flag = m_CachedLocale != LocalizationManager.Instance.CurrentLocale;
			if (m_CashedLocalizedValues == null || m_CashedLocalizedValues.Count == 0 || flag)
			{
				List<string> list = ((IEnumerable<LocalizedString>)values).Select((Func<LocalizedString, string>)((LocalizedString v) => v)).ToList();
				if (Application.isEditor || Debug.isDebugBuild)
				{
					list.Add(Locale.dev.ToString());
				}
				m_CashedLocalizedValues = list.ToArray();
				m_CachedLocale = LocalizationManager.Instance.CurrentLocale;
			}
			return m_CashedLocalizedValues;
		}
	}

	public override void SetIndexTempValue(int value)
	{
		SetTempValue(GetLocaleByIndex(value));
	}

	public override void SetIndexValueAndConfirm(int value)
	{
		SetValueAndConfirm(GetLocaleByIndex(value));
	}

	public override int GetIndexTempValue()
	{
		return GetIndexByLocale(GetTempValue());
	}

	private Locale GetLocaleByIndex(int value)
	{
		if (!m_IndexToLocaleMap.TryGetValue(value, out var value2))
		{
			return Locale.enGB;
		}
		return value2;
	}

	private int GetIndexByLocale(Locale value)
	{
		return m_IndexToLocaleMap.FirstOrDefault((KeyValuePair<int, Locale> x) => x.Value == value).Key;
	}
}
