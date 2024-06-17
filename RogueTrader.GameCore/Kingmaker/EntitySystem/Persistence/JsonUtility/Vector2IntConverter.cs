using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public class Vector2IntConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(Vector2Int) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		Vector2Int vector2Int = new Vector2Int(0, 0);
		JObject jObject = JObject.Load(reader);
		try
		{
			int x = jObject["x"].ToObject<int>();
			int y = jObject["y"].ToObject<int>();
			vector2Int.Set(x, y);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
			PFLog.Default.Warning("Failed to deserialize Vector2Int: " + jObject.ToString(Formatting.None));
		}
		return vector2Int;
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		Vector2Int vector2Int = (Vector2Int)value;
		JObject jObject = new JObject();
		jObject["x"] = vector2Int.x;
		jObject["y"] = vector2Int.y;
		jObject.WriteTo(writer);
	}
}
