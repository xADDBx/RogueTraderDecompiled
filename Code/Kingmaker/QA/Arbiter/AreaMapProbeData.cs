using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
public class AreaMapProbeData : ISpecificProbeData
{
	[JsonProperty]
	public string Area;

	[JsonProperty]
	public string Sample = "0";

	public AreaMapProbeData(string areaName)
	{
		Area = areaName;
	}

	[JsonConstructor]
	public AreaMapProbeData()
	{
	}
}
