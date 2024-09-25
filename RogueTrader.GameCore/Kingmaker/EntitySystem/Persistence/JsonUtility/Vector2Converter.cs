using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class Vector2Converter : JsonConverter
{
	public override bool CanRead => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(Vector2) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Unnecessary because CanRead is false.");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector2 vector = (Vector2)value;
		JObject jObject = new JObject();
		jObject["x"] = vector.x;
		jObject["y"] = vector.y;
		jObject.WriteTo(writer);
	}
}
