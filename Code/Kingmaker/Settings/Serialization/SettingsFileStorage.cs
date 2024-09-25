using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Kingmaker.Settings.Serialization;

public class SettingsFileStorage
{
	private readonly string m_Path;

	private static JsonSerializer Serializer => SettingsJsonSerializer.Serializer;

	public SettingsFileStorage(string path)
	{
		m_Path = path;
	}

	public void Save(IReadOnlyDictionary<string, object> dictionary)
	{
		using StreamWriter textWriter = File.CreateText(m_Path);
		Serializer.Serialize(textWriter, dictionary, typeof(Dictionary<string, object>));
	}

	public Dictionary<string, object> Load()
	{
		try
		{
			if (!File.Exists(m_Path))
			{
				return null;
			}
			using StreamReader reader = File.OpenText(m_Path);
			using JsonTextReader reader2 = new JsonTextReader(reader);
			return Serializer.Deserialize<Dictionary<string, object>>(reader2);
		}
		catch (Exception ex)
		{
			PFLog.Settings.Exception(ex);
			try
			{
				File.Delete(m_Path);
			}
			catch (Exception ex2)
			{
				PFLog.Settings.Exception(ex2);
			}
		}
		return null;
	}
}
