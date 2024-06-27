using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadDuplicatedIdListDataExtension
{
	public static string ToJson(this PayloadDuplicatedIdListData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
