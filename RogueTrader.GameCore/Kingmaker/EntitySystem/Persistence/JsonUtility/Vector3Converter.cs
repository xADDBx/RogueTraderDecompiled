using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class Vector3Converter : JsonConverter
{
	public override bool CanRead => false;

	public bool OutputFullType { get; set; }

	public override bool CanConvert(Type objectType)
	{
		return typeof(Vector3) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Unnecessary because CanRead is false.");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector3 vector = (Vector3)value;
		JObject jObject = new JObject();
		if (OutputFullType)
		{
			jObject["$type"] = typeof(Vector3).AssemblyQualifiedName;
		}
		jObject["x"] = vector.x;
		jObject["y"] = vector.y;
		jObject["z"] = vector.z;
		jObject.WriteTo(writer);
	}
}
