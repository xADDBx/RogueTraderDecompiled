using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
public class AreaProbeData : ISpecificProbeData
{
	[JsonProperty]
	public string Area;

	[JsonProperty]
	public string AreaPart;

	[JsonProperty]
	public string ActiveScene;

	[JsonProperty]
	public string Sample;

	[JsonProperty]
	public string StaticScene;

	[JsonProperty]
	public string AreaPartLightScenes;

	[JsonProperty]
	public Dictionary<string, string> CustomMeasurements = new Dictionary<string, string>();

	public void AddCustomMeasurements(Dictionary<string, string> measurements)
	{
		foreach (KeyValuePair<string, string> measurement in measurements)
		{
			CustomMeasurements[measurement.Key] = measurement.Value;
		}
	}
}
