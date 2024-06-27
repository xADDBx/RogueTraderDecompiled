using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadTypeIdListDataExtension
{
	public static string ToJson(this PayloadTypeIdListData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
