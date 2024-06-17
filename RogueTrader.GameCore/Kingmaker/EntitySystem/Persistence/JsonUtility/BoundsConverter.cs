using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class BoundsConverter : JsonConverter
{
	public override bool CanRead => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(Bounds) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Unnecessary because CanRead is false.");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		JsonSerializer jsonSerializer = new JsonSerializer();
		Bounds bounds = (Bounds)value;
		JObject jObject = new JObject();
		jObject["center"] = JToken.FromObject(bounds.center, jsonSerializer);
		jObject["extents"] = JToken.FromObject(bounds.extents, jsonSerializer);
		jObject.WriteTo(writer);
	}
}
