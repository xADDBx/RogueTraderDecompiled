using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadOkData
{
	[JsonProperty("ok")]
	public bool Ok => true;

	public static PayloadOkData FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadOkData>(json);
	}

	public static PayloadOkData Create()
	{
		return new PayloadOkData();
	}
}
