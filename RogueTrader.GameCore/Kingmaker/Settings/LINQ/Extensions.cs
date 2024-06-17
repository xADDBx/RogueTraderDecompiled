using System;
using Kingmaker.Settings.Entities;

namespace Kingmaker.Settings.LINQ;

public static class Extensions
{
	public static IReadOnlySettingEntity<TValue> Select<TSource, TValue>(this IReadOnlySettingEntity<TSource> source, Func<TSource, TValue> converter)
	{
		return new SelectSetting<TSource, TValue>(source, converter);
	}

	public static IReadOnlySettingEntity<bool> WasTouched<TSource>(this SettingsEntity<TSource> source)
	{
		return new WasTouchedSetting<TSource>(source);
	}
}
