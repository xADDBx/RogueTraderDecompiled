using System.Collections.Generic;
using Kingmaker.Settings.Serialization;

namespace Kingmaker.Settings;

public class GeneralSettingsProvider : DictionarySettingsProvider
{
	private readonly SettingsFileStorage m_FileStorage;

	private readonly Dictionary<string, object> m_Dictionary;

	protected override Dictionary<string, object> Dictionary => m_Dictionary;

	public GeneralSettingsProvider(string path)
	{
		m_FileStorage = new SettingsFileStorage(path);
		m_Dictionary = m_FileStorage.Load() ?? new Dictionary<string, object>();
		IsEmpty = m_Dictionary.Count == 0;
	}

	public override void SaveAll()
	{
		m_FileStorage.Save(m_Dictionary);
	}
}
