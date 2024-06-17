using System;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

[UsedImplicitly]
public class EntityRefConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(UnitReference)) && !(objectType == typeof(EntityRef)))
		{
			if (objectType.IsGenericType)
			{
				return objectType.GetGenericTypeDefinition() == typeof(EntityRef<>);
			}
			return false;
		}
		return true;
	}

	private string ReadId(JsonReader reader)
	{
		if (reader.TokenType == JsonToken.Null)
		{
			return null;
		}
		if (reader.Value is string result)
		{
			return result;
		}
		return (string?)JObject.Load(reader).Properties().First()
			.Value;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		string text = ReadId(reader);
		if (objectType == typeof(UnitReference))
		{
			return new UnitReference(text);
		}
		if (objectType == typeof(EntityRef))
		{
			return new EntityRef(text);
		}
		if (text != null)
		{
			return Activator.CreateInstance(objectType, text);
		}
		return null;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		if (!(value is UnitReference unitReference))
		{
			if (!(value is EntityRef entityRef))
			{
				if (value is ITypedEntityRef typedEntityRef)
				{
					writer.WriteValue(typedEntityRef.GetId());
				}
			}
			else
			{
				writer.WriteValue(entityRef.Id);
			}
		}
		else
		{
			writer.WriteValue(unitReference.Id);
		}
	}
}
