using System.Collections.Generic;
using Newtonsoft.Json;

namespace Code.Blueprints.JsonSystem.EditorDatabase.FileDatabaseClient.PayloadData;

public class PayloadContainsRemoveBlueprintsData
{
	[JsonProperty("contains_remove_blueprints_list")]
	public Dictionary<string, HashSet<string>> ContainsRemoveBlueprintsList { get; set; }

	public static PayloadContainsRemoveBlueprintsData? FromJson(string json)
	{
		return JsonConvert.DeserializeObject<PayloadContainsRemoveBlueprintsData>(json);
	}

	public static PayloadContainsRemoveBlueprintsData CreateEmpty()
	{
		return new PayloadContainsRemoveBlueprintsData();
	}

	public PayloadContainsRemoveBlueprintsData()
	{
		ContainsRemoveBlueprintsList = new Dictionary<string, HashSet<string>>();
	}

	public void Add(string removeBlueprintGuid, string blueprintGuid)
	{
		if (!ContainsRemoveBlueprintsList.ContainsKey(removeBlueprintGuid))
		{
			ContainsRemoveBlueprintsList.Add(removeBlueprintGuid, new HashSet<string>());
		}
		if (!ContainsRemoveBlueprintsList[removeBlueprintGuid].Contains(blueprintGuid))
		{
			ContainsRemoveBlueprintsList[removeBlueprintGuid].Add(blueprintGuid);
		}
	}
}
