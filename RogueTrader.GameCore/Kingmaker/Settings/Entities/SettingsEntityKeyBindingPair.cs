using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities;

public class SettingsEntityKeyBindingPair : SettingsEntity<KeyBindingPair>
{
	public SettingsEntityKeyBindingPair(ISettingsController settingsController, string key, KeyBindingPair defaultValue, bool saveDependent = false, bool requireReboot = false)
		: base(settingsController, key, defaultValue, saveDependent, requireReboot)
	{
	}

	public void SetTempKeyBindingData(KeyBindingData data, int index)
	{
		if (index == 0 || index == 1)
		{
			KeyBindingPair tempValue = GetTempValue();
			if (index == 0)
			{
				tempValue.Binding1 = data;
			}
			else
			{
				tempValue.Binding2 = data;
			}
			SetTempValue(tempValue);
		}
	}

	public void SetKeyBindingDataAndConfirm(KeyBindingData data, int index)
	{
		SetTempKeyBindingData(data, index);
		ConfirmTempValue();
	}
}
