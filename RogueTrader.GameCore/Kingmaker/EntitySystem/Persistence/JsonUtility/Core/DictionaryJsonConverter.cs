using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility.Core;

public class DictionaryJsonConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		IDictionary dictionary = (IDictionary)value;
		writer.WriteStartObject();
		foreach (object key in dictionary.Keys)
		{
			writer.WritePropertyName(key.ToString());
			serializer.Serialize(writer, dictionary[key]);
		}
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (!CanConvert(objectType))
		{
			throw new Exception($"This converter is not for {objectType}.");
		}
		Type type = objectType.GetGenericArguments()[0];
		Type type2 = objectType.GetGenericArguments()[1];
		Type type3 = typeof(Dictionary<, >).MakeGenericType(type, type2);
		IDictionary result = (type3.IsInstanceOfType(existingValue) ? ((IDictionary)existingValue) : ((IDictionary)Activator.CreateInstance(type3)));
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		reader.Read();
		while (reader.TokenType != JsonToken.EndObject)
		{
			AddObjectToDictionary(reader, result, serializer, type, type2);
		}
		return result;
	}

	public override bool CanConvert(Type objectType)
	{
		if (objectType.IsGenericType)
		{
			if (!(objectType.GetGenericTypeDefinition() == typeof(IDictionary<, >)))
			{
				return objectType.GetGenericTypeDefinition() == typeof(Dictionary<, >);
			}
			return true;
		}
		return false;
	}

	private static void AddObjectToDictionary(JsonReader reader, IDictionary result, JsonSerializer serializer, Type keyType, Type valueType)
	{
		using StringReader reader2 = new StringReader("\"" + reader.Value?.ToString() + "\"");
		using JsonTextReader reader3 = new JsonTextReader(reader2);
		object key = serializer.Deserialize(reader3, keyType);
		reader.Read();
		object value = serializer.Deserialize(reader, valueType);
		reader.Read();
		result.Add(key, value);
	}
}
