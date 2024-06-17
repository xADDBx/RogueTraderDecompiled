using System.Collections.Generic;
using System.IO;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.EntitySystem.Persistence;

public class SavedFogMasks
{
	private readonly Dictionary<string, byte[]> m_Masks = new Dictionary<string, byte[]>();

	public void Add(string key, byte[] data)
	{
		File.WriteAllBytes(Path.Combine(AreaDataStash.Folder, "__fog__" + key), data);
		m_Masks[key] = null;
	}

	public void Add(string key, string file)
	{
		string destFileName = Path.Combine(AreaDataStash.Folder, "__fog__" + key);
		File.Copy(file, destFileName, overwrite: true);
		m_Masks[key] = null;
	}

	public byte[] Get(string key)
	{
		if (m_Masks.ContainsKey(key))
		{
			string path = Path.Combine(AreaDataStash.Folder, "__fog__" + key);
			if (!File.Exists(path))
			{
				return null;
			}
			return File.ReadAllBytes(path);
		}
		return m_Masks.Get(key);
	}

	public void Clear()
	{
		foreach (string key in m_Masks.Keys)
		{
			string path = Path.Combine(AreaDataStash.Folder, "__fog__" + key);
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		m_Masks.Clear();
	}

	public void SaveAll(string folder, string prefix)
	{
		foreach (KeyValuePair<string, byte[]> mask in m_Masks)
		{
			string text = Path.Combine(AreaDataStash.Folder, "__fog__" + mask.Key);
			string destFileName = Path.Combine(folder, prefix + "." + mask.Key + ".fog");
			if (File.Exists(text))
			{
				File.Copy(text, destFileName, overwrite: true);
			}
		}
	}
}
