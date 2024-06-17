using System;

namespace Kingmaker.Settings.Entities;

public interface IReadOnlySettingEntity<TSettingsValue>
{
	event Action<TSettingsValue> OnTempValueChanged;

	event Action<TSettingsValue> OnValueChanged;

	TSettingsValue GetValue();

	TSettingsValue GetTempValue();
}
