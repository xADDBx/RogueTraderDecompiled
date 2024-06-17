using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class Matrix4x4Converter : JsonConverter
{
	public override bool CanRead => true;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		Matrix4x4 matrix4x = (Matrix4x4)value;
		writer.WriteStartArray();
		writer.WriteValue(matrix4x.m00);
		writer.WriteValue(matrix4x.m01);
		writer.WriteValue(matrix4x.m02);
		writer.WriteValue(matrix4x.m03);
		writer.WriteValue(matrix4x.m10);
		writer.WriteValue(matrix4x.m11);
		writer.WriteValue(matrix4x.m12);
		writer.WriteValue(matrix4x.m13);
		writer.WriteValue(matrix4x.m20);
		writer.WriteValue(matrix4x.m21);
		writer.WriteValue(matrix4x.m22);
		writer.WriteValue(matrix4x.m23);
		writer.WriteValue(matrix4x.m30);
		writer.WriteValue(matrix4x.m31);
		writer.WriteValue(matrix4x.m32);
		writer.WriteValue(matrix4x.m33);
		writer.WriteEnd();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return default(Matrix4x4);
		}
		JArray jArray = JArray.Load(reader);
		Matrix4x4 matrix4x = default(Matrix4x4);
		matrix4x.m00 = (float)jArray[0];
		matrix4x.m01 = (float)jArray[1];
		matrix4x.m02 = (float)jArray[2];
		matrix4x.m03 = (float)jArray[3];
		matrix4x.m10 = (float)jArray[4];
		matrix4x.m11 = (float)jArray[5];
		matrix4x.m12 = (float)jArray[6];
		matrix4x.m13 = (float)jArray[7];
		matrix4x.m20 = (float)jArray[8];
		matrix4x.m21 = (float)jArray[9];
		matrix4x.m22 = (float)jArray[10];
		matrix4x.m23 = (float)jArray[11];
		matrix4x.m30 = (float)jArray[12];
		matrix4x.m31 = (float)jArray[13];
		matrix4x.m32 = (float)jArray[14];
		matrix4x.m33 = (float)jArray[15];
		return matrix4x;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(Matrix4x4);
	}
}
