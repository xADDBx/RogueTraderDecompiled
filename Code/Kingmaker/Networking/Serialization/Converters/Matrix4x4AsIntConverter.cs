using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.Networking.Serialization.Converters;

public class Matrix4x4AsIntConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(Matrix4x4) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		JArray jArray = JArray.Load(reader);
		Matrix4x4 matrix4x = default(Matrix4x4);
		for (int i = 0; i < 16; i++)
		{
			matrix4x[i] = BitConverter.Int32BitsToSingle((int)jArray[i]);
		}
		return matrix4x;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Matrix4x4 matrix4x = (Matrix4x4)value;
		writer.WriteStartArray();
		for (int i = 0; i < 16; i++)
		{
			writer.WriteValue(BitConverter.SingleToInt32Bits(matrix4x[i]));
		}
		writer.WriteEnd();
	}
}
