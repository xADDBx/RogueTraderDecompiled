using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.Settings;

public class AccessiabilitySettings
{
	public readonly SettingsEntityFloat Protanopia;

	public readonly SettingsEntityFloat Deuteranopia;

	public readonly SettingsEntityFloat Tritanopia;

	private readonly SettingsEntityFloat m_FontSize;

	private readonly float m_DefaultFontSizeConsole;

	public float FontSizeMultiplier => GetFontSizeMultiplier();

	public AccessiabilitySettings(ISettingsController settingsController, SettingsValues settingsValues)
	{
		AccessiabilitySettingsDefaultValues accessiability = settingsValues.SettingsDefaultValues.Accessiability;
		Protanopia = new SettingsEntityFloat(settingsController, "protanopia", accessiability.Protanopia);
		Deuteranopia = new SettingsEntityFloat(settingsController, "deuteranopia", accessiability.Deuteranopia);
		Tritanopia = new SettingsEntityFloat(settingsController, "tritanopia", accessiability.Tritanopia);
		m_DefaultFontSizeConsole = accessiability.FontSizeConsole;
		m_FontSize = new SettingsEntityFloat(settingsController, "font-size", (Application.isConsolePlatform || ApplicationHelper.IsRunOnSteamDeck) ? m_DefaultFontSizeConsole : accessiability.FontSizePC);
	}

	public SettingsEntityFloat GetFontSizeSettingsEntity()
	{
		return m_FontSize;
	}

	private float GetFontSizeMultiplier()
	{
		if (Application.isConsolePlatform || ApplicationHelper.IsRunOnSteamDeck)
		{
			return m_DefaultFontSizeConsole;
		}
		return m_FontSize.GetValue();
	}
}
