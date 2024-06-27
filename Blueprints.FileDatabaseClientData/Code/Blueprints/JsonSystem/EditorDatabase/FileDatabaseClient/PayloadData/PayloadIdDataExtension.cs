using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadIdDataExtension
{
	public static string ToJson(this PayloadIdData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
