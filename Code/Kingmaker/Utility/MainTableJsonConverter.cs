using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.Utility;

public class MainTableJsonConverter : JsonConverter<MainTable>
{
	public override bool CanRead => true;

	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, MainTable value, JsonSerializer serializer)
	{
		throw new NotImplementedException("No need to implement");
	}

	public override MainTable ReadJson(JsonReader reader, Type objectType, MainTable existingValue, bool valueExists, JsonSerializer serializer)
	{
		Dictionary<string, Dictionary<string, AssigneeQa>> dictionary = JToken.Load(reader).ToObject<Dictionary<string, Dictionary<string, AssigneeQa>>>();
		foreach (string key in dictionary.Keys)
		{
			if (!Enum.TryParse<BugContext.ContextType>(key, out var result))
			{
				continue;
			}
			foreach (string key2 in dictionary[key].Keys)
			{
				if (Enum.TryParse<BugContext.AspectType>(key2, out var result2))
				{
					existingValue.Assignees.TryAdd(result, new Dictionary<BugContext.AspectType, AssigneeQa>());
					existingValue.Assignees[result].TryAdd(result2, dictionary[key][key2]);
				}
			}
		}
		return existingValue;
	}
}
