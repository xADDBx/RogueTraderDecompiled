using System;
using Kingmaker.Blueprints;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class BlueprintConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return objectType.IsSubclassOf(typeof(SimpleBlueprint));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		string text = (string)reader.Value;
		if (text == null || text == "null")
		{
			return null;
		}
		return ResourcesLibrary.TryGetBlueprint(text) ?? throw new JsonSerializationException("Failed to load blueprint by guid " + text);
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		SimpleBlueprint simpleBlueprint = value as SimpleBlueprint;
		if (simpleBlueprint != null && string.IsNullOrEmpty(simpleBlueprint.AssetGuid))
		{
			PFLog.Default.Error("Bad blueprint " + simpleBlueprint.name + ": has no AssetGuid!", simpleBlueprint);
		}
		writer.WriteValue((simpleBlueprint == null) ? "null" : simpleBlueprint.AssetGuid);
	}
}
