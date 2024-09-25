using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
public class AreaMapExportData : ISpecificProbeData
{
	[JsonProperty]
	public string BoundsCenter;

	[JsonProperty]
	public string BoundsSize;

	[JsonProperty]
	public string Sample;

	[JsonConstructor]
	public AreaMapExportData()
	{
	}
}
