using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadContainsRemoveBlueprintsDataExtension
{
	public static string ToJson(this PayloadContainsRemoveBlueprintsData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
