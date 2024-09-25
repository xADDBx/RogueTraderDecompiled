using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class QuaternionConverter : JsonConverter
{
	public override bool CanRead => false;

	public bool OutputFullType { get; set; }

	public override bool CanConvert(Type objectType)
	{
		return typeof(Quaternion) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		throw new InvalidOperationException("Unnecessary because CanRead is false.");
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Quaternion quaternion = (Quaternion)value;
		JObject jObject = new JObject();
		if (OutputFullType)
		{
			jObject["$type"] = typeof(Vector3).AssemblyQualifiedName;
		}
		jObject["x"] = quaternion.x;
		jObject["y"] = quaternion.y;
		jObject["z"] = quaternion.z;
		jObject["w"] = quaternion.w;
		jObject.WriteTo(writer);
	}
}
