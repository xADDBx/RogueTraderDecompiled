using System;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class StringOrIntEnumConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (value == null)
		{
			writer.WriteNull();
			return;
		}
		string text = ((Enum)value).ToString("G");
		if (char.IsNumber(text[0]) || text[0] == '-')
		{
			writer.WriteValue(value);
		}
		else
		{
			writer.WriteValue(text);
		}
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		switch (reader.TokenType)
		{
		case JsonToken.Null:
			return Activator.CreateInstance(objectType);
		case JsonToken.String:
			try
			{
				return Enum.Parse(GetExactTypeToDeserialize(objectType) ?? throw new Exception($"Invalid type {objectType}"), (string)reader.Value);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2, "Failed to deserialize enum of type {0} from \"{1}\" while reading {2}", objectType, reader.Value, Json.BlueprintBeingRead?.AssetId ?? "unknown");
				return Activator.CreateInstance(objectType);
			}
		case JsonToken.Integer:
			try
			{
				Type enumType = GetExactTypeToDeserialize(objectType) ?? throw new Exception($"Invalid type {objectType}");
				long value = (long)reader.Value;
				return Enum.ToObject(enumType, value);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex, "Failed to deserialize enum of type {0} from \"{1}\" while reading {2}", objectType, reader.Value, Json.BlueprintBeingRead?.AssetId ?? "unknown");
				return Activator.CreateInstance(objectType);
			}
		default:
			throw new JsonSerializationException($"Cannot read {reader.TokenType} as enum value");
		}
	}

	public override bool CanConvert(Type objectType)
	{
		return GetExactTypeToDeserialize(objectType) != null;
	}

	private static Type GetExactTypeToDeserialize(Type maybeNullableType)
	{
		while (true)
		{
			if (maybeNullableType.IsEnum)
			{
				return maybeNullableType;
			}
			if (!maybeNullableType.IsGenericType || maybeNullableType.GetGenericTypeDefinition() != typeof(Nullable<>))
			{
				break;
			}
			maybeNullableType = maybeNullableType.GetGenericArguments()[0];
		}
		return null;
	}
}
