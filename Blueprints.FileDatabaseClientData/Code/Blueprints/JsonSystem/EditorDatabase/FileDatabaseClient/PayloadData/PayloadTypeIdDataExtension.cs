using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadTypeIdDataExtension
{
	public static string ToJson(this PayloadTypeIdData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
