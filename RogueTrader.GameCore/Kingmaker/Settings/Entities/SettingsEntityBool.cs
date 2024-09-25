using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities;

public class SettingsEntityBool : SettingsEntity<bool>
{
	public SettingsEntityBool(ISettingsController settingsController, string key, bool defaultValue, bool saveDependent = false, bool requireReboot = false)
		: base(settingsController, key, defaultValue, saveDependent, requireReboot)
	{
	}
}
