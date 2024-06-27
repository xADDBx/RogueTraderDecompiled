using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadNameDataExtension
{
	public static string ToJson(this PayloadNameData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
