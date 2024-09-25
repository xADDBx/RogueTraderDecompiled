using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public class GameTooltipsSettings
{
	public readonly SettingsEntityBool ShowComparative;

	public readonly SettingsEntityFloat ShowDelay;

	public readonly SettingsEntityBool Shortened;

	public GameTooltipsSettings(ISettingsController settingsController, GameTooltipsSettingsDefaultValues defaultValues)
	{
		ShowComparative = new SettingsEntityBool(settingsController, "show-comparative", defaultValues.ShowComparative);
		ShowDelay = new SettingsEntityFloat(settingsController, "show-delay", defaultValues.ShowDelay);
		Shortened = new SettingsEntityBool(settingsController, "shortened", defaultValues.Shortened);
	}
}
