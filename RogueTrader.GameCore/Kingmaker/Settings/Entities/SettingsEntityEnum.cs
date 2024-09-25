using System;
using Kingmaker.Settings.Interfaces;
using Kingmaker.Utility.Enum;

namespace Kingmaker.Settings.Entities;

public class SettingsEntityEnum<TEnum> : SettingsEntity<TEnum>, ISettingsEntityEnum where TEnum : Enum, IConvertible
{
	public SettingsEntityEnum(ISettingsController settingsController, string key, TEnum defaultValue, bool saveDependent = false, bool requireReboot = false)
		: base(settingsController, key, defaultValue, saveDependent, requireReboot)
	{
	}

	public static implicit operator int(SettingsEntityEnum<TEnum> setting)
	{
		TEnum value = setting.GetValue();
		return EnumHelper<TEnum>.Instance.ToInt32(value);
	}

	int ISettingsEntityEnum.GetTempValue()
	{
		TEnum tempValue = GetTempValue();
		return EnumHelper<TEnum>.Instance.ToInt32(tempValue);
	}

	void ISettingsEntityEnum.SetValueAndConfirm(int value)
	{
		TEnum valueAndConfirm = EnumHelper<TEnum>.Instance.FromInt32(value);
		SetValueAndConfirm(valueAndConfirm);
	}
}
