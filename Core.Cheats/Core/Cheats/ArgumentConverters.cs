using System;
using System.Globalization;

namespace Core.Cheats;

internal static class ArgumentConverters
{
	[CheatArgConverter]
	internal static (bool success, object result) StringToEnumConverter(string value, Type targetType)
	{
		if (!targetType.IsEnum)
		{
			return (success: false, result: null);
		}
		string[] enumNames = targetType.GetEnumNames();
		Array enumValues = targetType.GetEnumValues();
		if (int.TryParse(value, out var result) && Enum.IsDefined(targetType, result))
		{
			return (success: true, result: Enum.ToObject(targetType, result));
		}
		for (int i = 0; i < enumNames.Length; i++)
		{
			if (string.Equals(enumNames[i], value, StringComparison.OrdinalIgnoreCase))
			{
				return (success: true, result: enumValues.GetValue(i));
			}
		}
		throw new ArgumentException("Cannot convert value " + value + " to enum " + targetType.FullName);
	}

	[CheatArgConverter]
	internal static (bool success, object result) StringToBoolConverter(string value, Type targetType)
	{
		if (targetType != typeof(bool))
		{
			return (success: false, result: null);
		}
		if (string.Compare(value, "0", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(value, "false", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return (success: true, result: false);
		}
		if (string.Compare(value, "1", StringComparison.OrdinalIgnoreCase) == 0 || string.Compare(value, "true", StringComparison.OrdinalIgnoreCase) == 0)
		{
			return (success: true, result: true);
		}
		throw new ArgumentException("Cannot convert value " + value + " to bool");
	}

	[CheatArgConverter]
	internal static (bool success, object result) StringToGuidConverter(string value, Type targetType)
	{
		if (targetType != typeof(Guid))
		{
			return (success: false, result: null);
		}
		if (Guid.TryParse(value, out var result))
		{
			return (success: true, result: result);
		}
		throw new ArgumentException("Cannot convert value " + value + " to guid");
	}

	[CheatArgConverter(Order = -1)]
	internal static (bool success, object result) NullConverter(string value, Type targetType)
	{
		if (string.Compare(value, "null", StringComparison.InvariantCultureIgnoreCase) != 0)
		{
			return (success: false, result: "");
		}
		if (targetType.IsValueType)
		{
			return (success: false, result: null);
		}
		return (success: true, result: null);
	}

	[CheatArgConverter(Order = 1)]
	internal static (bool success, object result) DefaultConverter(string value, Type targetType)
	{
		try
		{
			return (success: true, result: Convert.ChangeType(value, targetType, CultureInfo.InvariantCulture));
		}
		catch (Exception innerException)
		{
			throw new ArgumentException($"Cannot convert value {value} to type {targetType}", innerException);
		}
	}
}
