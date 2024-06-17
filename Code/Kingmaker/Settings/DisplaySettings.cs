using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class DisplaySettings
{
	public readonly SettingsEntityFloat GammaCorrection;

	public readonly SettingsEntityFloat Brightness;

	public readonly SettingsEntityFloat Contrast;

	public readonly SettingsEntityInt SafeZoneOffset;

	public DisplaySettings(ISettingsController settingsController, SettingsValues settingsValues)
	{
		DisplaySettingsDefaultValues display = settingsValues.SettingsDefaultValues.Display;
		GammaCorrection = new SettingsEntityFloat(settingsController, "gamma-correction", display.GammaCorrection);
		Brightness = new SettingsEntityFloat(settingsController, "brightness", display.Brightness);
		Contrast = new SettingsEntityFloat(settingsController, "contrast", display.Contrast);
		SafeZoneOffset = new SettingsEntityInt(settingsController, "safe-zone-offset", display.SafeZoneOffset);
	}
}
