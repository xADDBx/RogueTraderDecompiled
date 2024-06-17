using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class Color32Converter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Color32 color = (Color32)value;
		int value2 = color.r + (color.g << 8) + (color.b << 16) + (color.a << 24);
		writer.WriteValue(value2);
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		int num = Convert.ToInt32(reader.Value);
		return new Color32((byte)((uint)num & 0xFFu), (byte)((uint)(num >> 8) & 0xFFu), (byte)((uint)(num >> 16) & 0xFFu), (byte)((uint)(num >> 24) & 0xFFu));
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Color32);
	}
}
