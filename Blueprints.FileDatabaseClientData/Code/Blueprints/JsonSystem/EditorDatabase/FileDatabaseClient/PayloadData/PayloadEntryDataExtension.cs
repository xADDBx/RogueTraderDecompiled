using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadEntryDataExtension
{
	public static string ToJson(this PayloadEntryData listData)
	{
		return JsonConvert.SerializeObject(listData);
	}
}
