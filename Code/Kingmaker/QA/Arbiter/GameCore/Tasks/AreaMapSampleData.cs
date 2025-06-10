using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter.GameCore.Tasks;

[JsonObject]
public class AreaMapSampleData : ISampleData
{
	[JsonProperty]
	public string BoundsCenter;

	[JsonProperty]
	public string BoundsSize;

	[JsonProperty]
	public string Sample;

	[JsonIgnore]
	public Dictionary<string, string> Data { get; set; }

	[JsonConstructor]
	public AreaMapSampleData()
	{
	}
}
