namespace Kingmaker.Settings.Interfaces;

public static class SettingsProviderExtensions
{
	public static bool TryGetValue<TSettingsValue>(this ISettingsProvider provider, string key, out TSettingsValue value)
	{
		if (provider.HasKey(key))
		{
			value = provider.GetValue<TSettingsValue>(key);
			return true;
		}
		value = default(TSettingsValue);
		return false;
	}

	public static bool SetValueIfNotExists<TSettingsValue>(this ISettingsProvider provider, string key, TSettingsValue value)
	{
		if (!provider.HasKey(key))
		{
			provider.SetValue(key, value);
			return true;
		}
		return false;
	}
}
