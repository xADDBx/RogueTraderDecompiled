using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public static class PayloadPathDataExtension
{
	public static string ToJson(this PayloadPathData data)
	{
		return JsonConvert.SerializeObject(data);
	}
}
