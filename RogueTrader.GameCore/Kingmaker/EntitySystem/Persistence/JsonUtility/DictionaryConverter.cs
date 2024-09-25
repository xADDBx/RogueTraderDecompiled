using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class DictionaryConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		if (!objectType.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = objectType.GetGenericTypeDefinition();
		if (!(typeof(Dictionary<, >) == genericTypeDefinition))
		{
			return typeof(IDictionary<, >) == genericTypeDefinition;
		}
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (JsonToken.Null == reader.TokenType)
		{
			return null;
		}
		IDictionary dictionary = (IDictionary)Activator.CreateInstance(objectType);
		Type[] genericArguments = objectType.GetGenericArguments();
		foreach (JObject item in JArray.Load(reader))
		{
			object obj;
			try
			{
				obj = item["Key"].ToObject(genericArguments[0], serializer);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				PFLog.Default.Warning("Failed to deserialize dictionary key (" + genericArguments[0].Name + "): " + item.ToString(Formatting.None));
				continue;
			}
			object value;
			try
			{
				value = item["Value"].ToObject(genericArguments[1], serializer);
			}
			catch (Exception ex2)
			{
				PFLog.Default.Exception(ex2);
				PFLog.Default.Warning("Failed to deserialize dictionary value (" + genericArguments[1].Name + "): " + item.ToString(Formatting.None));
				continue;
			}
			if (obj == null)
			{
				PFLog.Default.Warning("Dectionary key deserialized to null: {0}", item.ToString(Formatting.None));
			}
			else if (!dictionary.Contains(obj))
			{
				dictionary.Add(obj, value);
			}
			else
			{
				PFLog.Default.Warning("Ignore pair with repeat key: {0}", item.ToString(Formatting.None));
			}
		}
		return dictionary;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		serializer.Serialize(writer, ((IDictionary)value).Cast<object>());
	}
}
