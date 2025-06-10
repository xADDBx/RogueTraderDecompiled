using System.Collections.Generic;
using Newtonsoft.Json;

namespace Kingmaker.QA.Arbiter.GameCore.DynamicsChecker;

[JsonObject]
public class DynamicSampleData : ISampleData
{
	[JsonProperty(PropertyName = "CustomMeasurements")]
	public Dictionary<string, string> Data { get; set; } = new Dictionary<string, string>();


	public void AddCustomMeasurements(Dictionary<string, string> measurements)
	{
		foreach (KeyValuePair<string, string> measurement in measurements)
		{
			Data[measurement.Key] = measurement.Value;
		}
	}
}
