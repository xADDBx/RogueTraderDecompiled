using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadContainsShadowDeletedBlueprintsDataExtension
{
	public static string ToJson(this PayloadContainsShadowDeletedBlueprintsData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
