using System;
using Kingmaker.Blueprints.Base;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class BlueprintReferenceConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		string text = null;
		if (value is IReferenceBase referenceBase)
		{
			text = referenceBase.Guid;
		}
		else
		{
			writer.WriteNull();
		}
		if (string.IsNullOrEmpty(text))
		{
			writer.WriteNull();
		}
		else
		{
			writer.WriteValue("!bp_" + text);
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		object obj = Activator.CreateInstance(objectType);
		if (reader.TokenType == JsonToken.Null)
		{
			return obj;
		}
		string text = (string)reader.Value;
		if (text.StartsWith("!bp_"))
		{
			text = text.Substring(4);
			if (obj is IReferenceBase referenceBase)
			{
				referenceBase.ReadGuidFromJson(text);
			}
		}
		return obj;
	}

	public override bool CanConvert(Type objectType)
	{
		return typeof(IReferenceBase).IsAssignableFrom(objectType);
	}
}
