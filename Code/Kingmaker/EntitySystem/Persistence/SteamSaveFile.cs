using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence;

[JsonObject(IsReference = false)]
public class SteamSaveFile
{
	[JsonProperty]
	public string Filename;

	[JsonProperty]
	public int Version;
}
