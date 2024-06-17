using System;
using Kingmaker.Settings.Entities;
using Kingmaker.Settings.Interfaces;
using UniRx;

namespace Kingmaker.Utility.UniRxExtensions;

public static class SettingsExtensionMethods
{
	public static ReadOnlyReactiveProperty<TValue> ObserveValue<TValue>(this SettingsEntity<TValue> settingsEntity)
	{
		return Observable.FromEvent(delegate(Action<TValue> h)
		{
			settingsEntity.OnValueChanged += h;
		}, delegate(Action<TValue> h)
		{
			settingsEntity.OnValueChanged -= h;
		}).ToReadOnlyReactiveProperty(settingsEntity.GetTempValue());
	}

	public static ReadOnlyReactiveProperty<TValue> ObserveTempValue<TValue>(this SettingsEntity<TValue> settingsEntity)
	{
		return Observable.FromEvent(delegate(Action<TValue> h)
		{
			settingsEntity.OnTempValueChanged += h;
		}, delegate(Action<TValue> h)
		{
			settingsEntity.OnTempValueChanged -= h;
		}).ToReadOnlyReactiveProperty(settingsEntity.GetTempValue());
	}

	public static ReadOnlyReactiveProperty<bool> ObserveTempValueIsConfirmed(this ISettingsEntity settingsEntity)
	{
		return Observable.FromEvent(delegate(Action<bool> h)
		{
			settingsEntity.OnTempValueIsConfirmed += h;
		}, delegate(Action<bool> h)
		{
			settingsEntity.OnTempValueIsConfirmed -= h;
		}).ToReadOnlyReactiveProperty(settingsEntity.TempValueIsConfirmed);
	}
}
