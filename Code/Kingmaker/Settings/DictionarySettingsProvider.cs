using System;
using System.Collections.Generic;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;

namespace Kingmaker.Settings;

public abstract class DictionarySettingsProvider : ISettingsProvider
{
	protected abstract Dictionary<string, object> Dictionary { get; }

	public virtual bool IsEmpty { get; protected set; }

	public bool HasKey(string key)
	{
		return Dictionary.ContainsKey(key);
	}

	public void RemoveKey(string key)
	{
		Dictionary.Remove(key);
		if (Dictionary.Count == 0)
		{
			IsEmpty = true;
		}
	}

	public void SetValue<TSettingsValue>(string key, TSettingsValue value)
	{
		object value2 = ((!(value is KeyBindingPair keyBindingPair)) ? ((!(value is Enum)) ? ((object)value) : value.ToString()) : keyBindingPair.ToString());
		Dictionary[key] = value2;
		IsEmpty = false;
	}

	public TSettingsValue GetValue<TSettingsValue>(string key)
	{
		Dictionary.TryGetValue(key, out var value);
		if (value == null)
		{
			throw new NullReferenceException("[Settings] Don't have setting for key '" + key + "'");
		}
		if (value is TSettingsValue)
		{
			return (TSettingsValue)value;
		}
		if (value is string text)
		{
			if (KeyBindingPair.IsKeyBindingString(text))
			{
				return (TSettingsValue)(object)new KeyBindingPair(text);
			}
			try
			{
				return (TSettingsValue)Enum.Parse(typeof(TSettingsValue), text);
			}
			catch (Exception ex)
			{
				PFLog.Settings.Error(ex.ToString());
				PFLog.Settings.Error($"Can't cast {text} to Enum type {typeof(TSettingsValue)}. Setting default.");
				SetValue(key, default(TSettingsValue));
				return default(TSettingsValue);
			}
		}
		try
		{
			return (TSettingsValue)Convert.ChangeType(value, typeof(TSettingsValue));
		}
		catch (Exception ex2)
		{
			PFLog.Settings.Error(ex2.ToString());
			PFLog.Settings.Error($"Can't cast {value} to type {typeof(TSettingsValue)}. Setting default.");
			return default(TSettingsValue);
		}
	}

	public abstract void SaveAll();
}
