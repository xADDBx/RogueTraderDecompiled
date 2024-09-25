using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence;

[JsonObject(IsReference = false)]
public class SteamSavesRegistry
{
	[JsonProperty]
	public int Version;

	[JsonProperty]
	public List<SteamSaveFile> Files = new List<SteamSaveFile>();

	public void RegisterSave(string filename)
	{
		Version++;
		foreach (SteamSaveFile file in Files)
		{
			if (file.Filename == filename)
			{
				file.Version = Version;
				return;
			}
		}
		SteamSaveFile item = new SteamSaveFile
		{
			Filename = filename,
			Version = Version
		};
		Files.Add(item);
	}

	public void DeleteSave(string filename)
	{
		Files.RemoveAll((SteamSaveFile f) => f.Filename == filename);
	}

	public static List<string> GetChangedFiles(SteamSavesRegistry before, SteamSavesRegistry after)
	{
		List<string> list = new List<string>();
		Dictionary<string, int> dictionary = new Dictionary<string, int>();
		foreach (SteamSaveFile file in before.Files)
		{
			dictionary[file.Filename] = file.Version;
		}
		foreach (SteamSaveFile file2 in after.Files)
		{
			if (file2.Version > dictionary.Get(file2.Filename, 0))
			{
				list.Add(file2.Filename);
			}
		}
		return list;
	}
}
