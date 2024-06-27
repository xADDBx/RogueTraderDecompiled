using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadTypeDataExtension
{
	public static string ToJson(this PayloadTypeData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
