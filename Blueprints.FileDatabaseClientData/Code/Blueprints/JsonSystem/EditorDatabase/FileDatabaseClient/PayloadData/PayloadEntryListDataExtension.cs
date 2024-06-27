using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadEntryListDataExtension
{
	public static string ToJson(this PayloadEntryListData listData)
	{
		return JsonConvert.SerializeObject(listData);
	}
}
