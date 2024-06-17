using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class RectConverter : JsonConverter
{
	public override bool CanRead => false;

	public override bool CanConvert(Type objectType)
	{
		return typeof(Rect) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Unnecessary because CanRead is false.");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Rect rect = (Rect)value;
		JObject jObject = new JObject();
		jObject["x"] = rect.x;
		jObject["y"] = rect.y;
		jObject["width"] = rect.width;
		jObject["height"] = rect.height;
		jObject.WriteTo(writer);
	}
}
