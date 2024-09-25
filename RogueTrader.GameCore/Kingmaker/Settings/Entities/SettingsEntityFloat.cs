using System;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities;

public class SettingsEntityFloat : SettingsEntity<float>
{
	private const float TOLERANCE = 0.001f;

	public SettingsEntityFloat(ISettingsController settingsController, string key, float defaultValue, bool saveDependent = false, bool requireReboot = false)
		: base(settingsController, key, defaultValue, saveDependent, requireReboot)
	{
	}

	protected override bool ValueEquals(float value1, float value2)
	{
		return Math.Abs(value1 - value2) < 0.001f;
	}
}
