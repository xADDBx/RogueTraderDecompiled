using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.Networking.Serialization.Converters;

public class Vector3NullableConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(Vector3?) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		JArray jArray = JArray.Load(reader);
		Vector3 vector = default(Vector3);
		vector.x = (float)jArray[0];
		vector.y = (float)jArray[1];
		vector.z = (float)jArray[2];
		return vector;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector3? vector = (Vector3?)value;
		if (!vector.HasValue)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartArray();
		writer.WriteValue(vector.Value.x);
		writer.WriteValue(vector.Value.y);
		writer.WriteValue(vector.Value.z);
		writer.WriteEnd();
	}
}
