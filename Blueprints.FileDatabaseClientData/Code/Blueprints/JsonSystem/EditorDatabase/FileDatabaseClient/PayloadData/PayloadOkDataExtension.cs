using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadOkDataExtension
{
	public static string ToJson(this PayloadOkData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
