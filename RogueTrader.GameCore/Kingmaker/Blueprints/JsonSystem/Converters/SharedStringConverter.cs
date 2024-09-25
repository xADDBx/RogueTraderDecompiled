using System;
using System.Reflection;
using Kingmaker.Localization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class SharedStringConverter : JsonConverter
{
	private static MethodInfo CurrentThreadIsMainThread = typeof(UnityEngine.Object).GetMethod("CurrentThreadIsMainThread", BindingFlags.Static | BindingFlags.NonPublic);

	private static bool IsMainThread => (bool)CurrentThreadIsMainThread.Invoke(null, null);

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		SharedStringAsset sharedStringAsset = value as SharedStringAsset;
		if (sharedStringAsset == null)
		{
			writer.WriteNull();
			return;
		}
		writer.WriteStartObject();
		writer.WritePropertyName("stringkey");
		writer.WriteValue(sharedStringAsset.String.Key);
		writer.WriteEndObject();
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		string key = (string?)JObject.Load(reader)["stringkey"];
		return new SharedStringAsset
		{
			String = new LocalizedString
			{
				Key = key
			}
		};
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(SharedStringAsset);
	}
}
