using System;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.UI.Models.SettingsUI.SettingAssets;

public class UISettingsEntityWithValueBase<TValue> : UISettingsEntityBase, IUISettingsEntityWithValueBase, IUISettingsEntityBase
{
	public SettingsEntity<TValue> Setting;

	[HideInInspector]
	public Func<bool> ModificationAllowedCheck;

	public ISettingsEntity SettingsEntity
	{
		get
		{
			if (Setting == null)
			{
				UberDebug.LogError("Setting " + base.name + " is not initialize");
				return null;
			}
			return Setting;
		}
	}

	public bool ModificationAllowed => ModificationAllowedCheck?.Invoke() ?? true;

	public string ModificationAllowedReason { get; set; }

	public bool IsSaved => Setting.TempValueIsConfirmed;

	public virtual void LinkSetting(SettingsEntity<TValue> setting)
	{
		Setting = setting;
	}

	public TValue GetTempValue()
	{
		if (Setting == null)
		{
			UberDebug.LogError("Setting " + base.name + " is not initialized");
			return default(TValue);
		}
		return Setting.GetTempValue();
	}

	public void SetTempValue(TValue value)
	{
		Setting.SetTempValue(value);
	}

	public void SetValueAndConfirm(TValue value)
	{
		Setting.SetValueAndConfirm(value);
	}

	public virtual void ResetToDefault()
	{
		Setting.ResetToDefault();
	}
}
