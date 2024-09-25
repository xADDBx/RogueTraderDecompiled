using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter;

[JsonObject]
public class DynamicProbeData : ISpecificProbeData
{
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
