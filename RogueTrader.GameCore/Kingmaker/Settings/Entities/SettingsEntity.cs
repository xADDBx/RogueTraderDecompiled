using System;
using System.Text.RegularExpressions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings.ConstructionHelpers.KeyPrefix;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings.Entities;

public abstract class SettingsEntity<TSettingsValue> : IReadOnlySettingEntity<TSettingsValue>, ISettingsEntity
{
	public readonly TSettingsValue DefaultValue;

	private bool m_CurrentValueIsSet;

	private TSettingsValue m_CurrentValue;

	private bool m_TempValueIsSet;

	private bool m_TempValueIsConfirmed = true;

	private TSettingsValue m_TempValue;

	public readonly ISettingsController settingsController;

	private TSettingsValue m_OverrideValue;

	private bool m_IsValueOverriden;

	public string Key { get; }

	public bool SaveDependent { get; }

	public bool RequireReboot { get; }

	public bool IsOverriden => m_IsValueOverriden;

	public bool TempValueIsConfirmed
	{
		get
		{
			return m_TempValueIsConfirmed;
		}
		private set
		{
			if (value != m_TempValueIsConfirmed)
			{
				m_TempValueIsConfirmed = value;
				this.OnTempValueIsConfirmed?.Invoke(m_TempValueIsConfirmed);
			}
		}
	}

	public event Action<TSettingsValue> OnTempValueChanged;

	public event Action<TSettingsValue> OnValueChanged;

	public event Action<bool> OnTempValueIsConfirmed;

	protected SettingsEntity(ISettingsController settingsController, string key, TSettingsValue defaultValue, bool saveDependent = false, bool requireReboot = false)
	{
		this.settingsController = settingsController;
		if (!Regex.IsMatch(key, "^[0-9a-z-.]+$"))
		{
			throw new Exception("[Settings] Key can contain only lower letter, numbers, dashes (-) and dots (.)");
		}
		if (SettingsKeyPrefix.HasPrefix)
		{
			Key = SettingsKeyPrefix.Prefix + key;
		}
		else
		{
			Key = key;
		}
		DefaultValue = defaultValue;
		SaveDependent = saveDependent;
		RequireReboot = requireReboot;
		this.settingsController.AccountSetting(this);
	}

	protected virtual bool ValueEquals(TSettingsValue value1, TSettingsValue value2)
	{
		return object.Equals(value1, value2);
	}

	public void SetTempValue(TSettingsValue value)
	{
		if (m_TempValueIsSet && ValueEquals(m_TempValue, value))
		{
			return;
		}
		m_TempValueIsSet = true;
		m_TempValue = value;
		TempValueIsConfirmed = false;
		if (ValueEquals(m_TempValue, GetValue()))
		{
			settingsController.RemoveFromConfirmationList(this, confirming: false);
			TempValueIsConfirmed = true;
		}
		else
		{
			if (!settingsController.ConfirmationListContains(this))
			{
				settingsController.AddToConfirmationList(this);
			}
			TempValueIsConfirmed = false;
		}
		this.OnTempValueChanged?.Invoke(m_TempValue);
		EventBus.RaiseEvent(delegate(IOptionsWindowUIHandler w)
		{
			w.HandleItemChanged(Key);
		});
	}

	public void SetValueAndConfirm(TSettingsValue value)
	{
		SetTempValue(value);
		ConfirmTempValue();
	}

	public TSettingsValue GetValue()
	{
		if (m_CurrentValueIsSet)
		{
			return m_CurrentValue;
		}
		TSettingsValue value;
		if (SaveDependent)
		{
			ISettingsProvider inSaveSettingsProvider = settingsController.InSaveSettingsProvider;
			if (inSaveSettingsProvider != null && inSaveSettingsProvider.TryGetValue<TSettingsValue>(Key, out value))
			{
				m_CurrentValue = value;
				goto IL_0077;
			}
		}
		ISettingsProvider generalSettingsProvider = settingsController.GeneralSettingsProvider;
		if (generalSettingsProvider != null && generalSettingsProvider.TryGetValue<TSettingsValue>(Key, out value))
		{
			m_CurrentValue = value;
		}
		else
		{
			m_CurrentValue = DefaultValue;
		}
		goto IL_0077;
		IL_0077:
		(SaveDependent ? settingsController.InSaveSettingsProvider : settingsController.GeneralSettingsProvider)?.SetValueIfNotExists(Key, m_CurrentValue);
		m_CurrentValueIsSet = true;
		if (TempValueIsConfirmed)
		{
			m_TempValue = m_CurrentValue;
		}
		return m_CurrentValue;
	}

	public TSettingsValue GetTempValue()
	{
		if (m_TempValueIsSet)
		{
			return m_TempValue;
		}
		return GetValue();
	}

	public void ResetToDefault(bool confirmChanges = false)
	{
		SetTempValue(DefaultValue);
		if (confirmChanges)
		{
			ConfirmTempValue();
		}
	}

	public void ConfirmTempValue()
	{
		TempValueIsConfirmed = true;
		m_CurrentValueIsSet = true;
		m_CurrentValue = m_TempValue;
		if (SaveDependent && settingsController.InSaveSettingsProvider != null)
		{
			settingsController.InSaveSettingsProvider.SetValue(Key, m_CurrentValue);
		}
		else
		{
			settingsController.GeneralSettingsProvider?.SetValue(Key, m_CurrentValue);
		}
		settingsController.RemoveFromConfirmationList(this, confirming: true);
		this.OnValueChanged?.Invoke(m_CurrentValue);
	}

	public void RevertTempValue()
	{
		if (m_TempValueIsSet && !TempValueIsConfirmed)
		{
			SetTempValue(GetValue());
		}
	}

	public void OverrideStart(TSettingsValue value)
	{
		m_IsValueOverriden = true;
		m_OverrideValue = value;
	}

	public void OverrideStop()
	{
		m_IsValueOverriden = false;
		m_OverrideValue = default(TSettingsValue);
	}

	public string GetStringValue()
	{
		return GetValue().ToString();
	}

	public string GetStringDefaultValue()
	{
		return DefaultValue.ToString();
	}

	public void SetCurrentValueInProvider()
	{
		if (SaveDependent && settingsController.InSaveSettingsProvider != null)
		{
			if (!settingsController.InSaveSettingsProvider.HasKey(Key))
			{
				settingsController.InSaveSettingsProvider.SetValue(Key, GetValue());
			}
		}
		else if (settingsController.GeneralSettingsProvider != null)
		{
			settingsController.GeneralSettingsProvider.SetValue(Key, GetValue());
		}
	}

	public void ResetCache()
	{
		m_TempValueIsSet = false;
		m_CurrentValueIsSet = false;
	}

	public void OnProviderValueChanged()
	{
		ResetCache();
		this.OnTempValueChanged?.Invoke(GetValue());
	}

	public bool CurrentValueIsNotDefault()
	{
		return !ValueEquals(GetTempValue(), DefaultValue);
	}

	public static implicit operator TSettingsValue(SettingsEntity<TSettingsValue> setting)
	{
		return setting.GetValue();
	}
}
