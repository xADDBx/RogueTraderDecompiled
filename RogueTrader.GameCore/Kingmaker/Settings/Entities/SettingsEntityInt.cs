using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities;

public class SettingsEntityInt : SettingsEntity<int>
{
	public SettingsEntityInt(ISettingsController settingsController, string key, int defaultValue, bool saveDependent = false, bool requireReboot = false)
		: base(settingsController, key, defaultValue, saveDependent, requireReboot)
	{
	}
}
