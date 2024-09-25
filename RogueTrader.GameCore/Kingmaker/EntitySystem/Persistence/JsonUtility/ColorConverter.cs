using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class ColorConverter : JsonConverter
{
	public override bool CanRead => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(Color) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Unnecessary because CanRead is false.");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Color color = (Color)value;
		JObject jObject = new JObject();
		jObject["r"] = color.r;
		jObject["g"] = color.g;
		jObject["b"] = color.b;
		jObject["a"] = color.a;
		jObject.WriteTo(writer);
	}
}
