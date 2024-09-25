using System;
using Kingmaker.Blueprints.Overrides;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class OverridesConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (!(value is OverridesManager overridesManager))
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartArray();
		overridesManager.ForEach(delegate(OverriddenProperty p)
		{
			if (p.IsOverrided)
			{
				writer.WriteValue(p.Path);
			}
		});
		writer.WriteEndArray();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return null;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(OverridesManager);
	}
}
