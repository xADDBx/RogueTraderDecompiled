using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence;

public struct KnownAreaEntry
{
	[JsonProperty]
	public string AreaGuid;

	[JsonProperty]
	public string[] AddStateScenes;

	public KnownAreaEntry(string areaGuid, string[] addStateScenes)
	{
		AreaGuid = areaGuid;
		AddStateScenes = addStateScenes;
	}
}
