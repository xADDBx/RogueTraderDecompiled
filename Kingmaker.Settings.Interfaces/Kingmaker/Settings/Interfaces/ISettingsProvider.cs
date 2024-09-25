namespace Kingmaker.Settings.Interfaces;

public interface ISettingsProvider
{
	bool IsEmpty { get; }

	bool HasKey(string key);

	void RemoveKey(string key);

	void SetValue<TSettingsValue>(string key, TSettingsValue value);

	TSettingsValue GetValue<TSettingsValue>(string key);

	void SaveAll();
}
