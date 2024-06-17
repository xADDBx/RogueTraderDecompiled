using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class PrototypeLinkConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (!(value as ScriptableObject))
		{
			writer.WriteNull();
		}
		else if (!(value is BlueprintScriptableObject blueprintScriptableObject))
		{
			if (value is BlueprintComponent { OwnerBlueprint: var ownerBlueprint } blueprintComponent)
			{
				writer.WriteStartObject();
				writer.WritePropertyName("guid");
				writer.WriteValue(ownerBlueprint.AssetGuid);
				writer.WritePropertyName("name");
				writer.WriteValue(blueprintComponent.name);
				writer.WriteEndObject();
			}
			else
			{
				writer.WriteNull();
			}
		}
		else
		{
			writer.WriteValue(blueprintScriptableObject.AssetGuid);
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return null;
	}

	public override bool CanConvert(Type objectType)
	{
		if (!objectType.IsSubclassOf(typeof(BlueprintScriptableObject)) && !objectType.IsSubclassOf(typeof(BlueprintComponent)))
		{
			return objectType == typeof(ScriptableObject);
		}
		return true;
	}
}
