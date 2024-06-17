using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility.Core;

public class CollectionsConverter : JsonConverter
{
	private static readonly bool s_Enabled;

	public override bool CanWrite => false;

	static CollectionsConverter()
	{
	}

	public override bool CanConvert(Type objectType)
	{
		if (!s_Enabled)
		{
			return false;
		}
		if (objectType.IsArray)
		{
			return true;
		}
		if (!objectType.IsGenericType)
		{
			return false;
		}
		Type genericTypeDefinition = objectType.GetGenericTypeDefinition();
		if (!(typeof(List<>) == genericTypeDefinition) && !(typeof(IList<>) == genericTypeDefinition))
		{
			return typeof(HashSet<>) == genericTypeDefinition;
		}
		return true;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		if (JsonToken.Null == reader.TokenType)
		{
			return null;
		}
		Type type = null;
		Type type2;
		if (objectType.IsArray)
		{
			type2 = objectType.GetElementType();
		}
		else
		{
			type = objectType.GetGenericTypeDefinition();
			type2 = objectType.GetGenericArguments()[0];
		}
		IList list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(type2));
		foreach (JToken item in JArray.Load(reader))
		{
			object value;
			try
			{
				value = item.ToObject(type2, serializer);
			}
			catch (Exception ex)
			{
				PFLog.Default.Exception(ex);
				PFLog.Default.Warning("Failed to deserialize collection element (" + type2?.Name + "): " + item.ToString(Formatting.None));
				continue;
			}
			list.Add(value);
		}
		if (objectType.IsArray)
		{
			Array array = Array.CreateInstance(type2, list.Count);
			for (int i = 0; i < list.Count; i++)
			{
				array.SetValue(list[i], i);
			}
			return array;
		}
		if (type == typeof(HashSet<>))
		{
			return Activator.CreateInstance(objectType, list);
		}
		return list;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException("Lists should be serialized using default methods");
	}
}
