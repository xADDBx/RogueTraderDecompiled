using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadNameListDataExtension
{
	public static string ToJson(this PayloadNameListData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
