using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadIsShadowDeletedDataExtension
{
	public static string ToJson(this PayloadIsShadowDeletedData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
