using System;
using System.ComponentModel;
using System.Globalization;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using UnityEngine;

namespace Kingmaker.Settings.Serialization;

public class PlayerPrefsSettingsProvider : ISettingsProvider
{
	public bool IsEmpty => false;

	public bool HasKey(string key)
	{
		return PlayerPrefs.HasKey(key);
	}

	public void RemoveKey(string key)
	{
		PlayerPrefs.DeleteKey(key);
	}

	public void SetValue<TSettingsValue>(string key, TSettingsValue value)
	{
		string value2 = Convert.ToString(value, CultureInfo.InvariantCulture);
		PlayerPrefs.SetString(key, value2);
	}

	public TSettingsValue GetValue<TSettingsValue>(string key)
	{
		string @string = PlayerPrefs.GetString(key, null);
		if (@string == null)
		{
			PFLog.Settings.Error("Null string setting for key " + key);
			return default(TSettingsValue);
		}
		if (typeof(TSettingsValue).IsAssignableFrom(typeof(Enum)))
		{
			try
			{
				return (TSettingsValue)Enum.Parse(typeof(TSettingsValue), @string);
			}
			catch (Exception ex)
			{
				PFLog.Settings.Error(ex.ToString());
				PFLog.Settings.Error($"Can't cast {@string} to Enum type {typeof(TSettingsValue)}. Setting default.");
				SetValue(key, default(TSettingsValue));
				return default(TSettingsValue);
			}
		}
		Vector2Int val;
		if (typeof(TSettingsValue).IsAssignableFrom(typeof(KeyBindingPair)))
		{
			if (KeyBindingPair.IsKeyBindingString(@string))
			{
				return (TSettingsValue)(object)new KeyBindingPair(@string);
			}
		}
		else if (typeof(TSettingsValue) == typeof(Vector2Int) && TryConvertVector2IntFromString(@string, out val))
		{
			return (TSettingsValue)(object)val;
		}
		try
		{
			return (TSettingsValue)TypeDescriptor.GetConverter(typeof(TSettingsValue)).ConvertFromInvariantString(@string);
		}
		catch (NotSupportedException ex2)
		{
			PFLog.Settings.Exception(ex2, "Failed to convert from string " + @string + " to type " + typeof(TSettingsValue).FullName);
			return default(TSettingsValue);
		}
	}

	public void SaveAll()
	{
		PlayerPrefs.Save();
	}

	private static bool TryConvertVector2IntFromString(string str, out Vector2Int val)
	{
		val = default(Vector2Int);
		int num = str.IndexOf('(');
		if (num == -1)
		{
			return false;
		}
		int num2 = str.IndexOf(", ", num, StringComparison.InvariantCulture);
		if (num2 == -1)
		{
			return false;
		}
		int num3 = str.IndexOf(')', num2);
		if (num3 == -1)
		{
			return false;
		}
		int num4 = num + 1;
		int num5 = num2 - num4;
		if (num5 < 1)
		{
			return false;
		}
		if (!int.TryParse(str.Substring(num4, num5), out var result))
		{
			return false;
		}
		int num6 = num2 + 2;
		int num7 = num3 - num6;
		if (num7 < 1)
		{
			return false;
		}
		if (!int.TryParse(str.Substring(num6, num7), out var result2))
		{
			return false;
		}
		val = new Vector2Int(result, result2);
		return true;
	}
}
