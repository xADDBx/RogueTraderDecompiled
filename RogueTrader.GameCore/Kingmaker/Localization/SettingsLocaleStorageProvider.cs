using System;
using Kingmaker.Localization.Enums;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Localization;

public class SettingsLocaleStorageProvider : ILocaleStorageProvider, IEquatable<SettingsLocaleStorageProvider>
{
	private readonly SettingsEntityEnum<Locale> m_LocalizationSettings;

	public Locale Locale
	{
		get
		{
			return m_LocalizationSettings.GetTempValue();
		}
		set
		{
			m_LocalizationSettings.SetTempValue(value);
		}
	}

	public event Action<Locale> Changed
	{
		add
		{
			m_LocalizationSettings.OnTempValueChanged += value;
		}
		remove
		{
			m_LocalizationSettings.OnTempValueChanged -= value;
		}
	}

	public SettingsLocaleStorageProvider(SettingsEntityEnum<Locale> localizationSettings)
	{
		m_LocalizationSettings = localizationSettings;
	}

	public bool Equals(SettingsLocaleStorageProvider other)
	{
		return object.Equals(m_LocalizationSettings, other?.m_LocalizationSettings);
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (this == obj)
		{
			return true;
		}
		if (obj.GetType() != GetType())
		{
			return false;
		}
		return Equals((SettingsLocaleStorageProvider)obj);
	}

	public override int GetHashCode()
	{
		if (m_LocalizationSettings == null)
		{
			return 0;
		}
		return m_LocalizationSettings.GetHashCode();
	}
}
