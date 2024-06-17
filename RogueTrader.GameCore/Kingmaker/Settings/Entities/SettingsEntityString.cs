using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities;

public class SettingsEntityString : SettingsEntity<string>
{
	public SettingsEntityString(ISettingsController settingsController, string key, string defaultValue, bool saveDependent = false, bool requireReboot = false)
		: base(settingsController, key, defaultValue, saveDependent, requireReboot)
	{
	}
}
