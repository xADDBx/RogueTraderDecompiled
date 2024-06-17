using System;
using Newtonsoft.Json;

namespace Kingmaker.Settings;

public class JsonInt32AndSingleConverter : JsonConverter
{
	public override bool CanWrite => false;

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		throw new NotImplementedException();
	}

	public override object ReadJson(JsonReader reader, Type type, object existingValue, JsonSerializer serializer)
	{
		return reader.TokenType switch
		{
			JsonToken.Integer => Convert.ToInt32(reader.Value), 
			JsonToken.Float => Convert.ToSingle(reader.Value), 
			_ => serializer.Deserialize(reader), 
		};
	}

	public override bool CanConvert(Type objectType)
	{
		if (!(objectType == typeof(long)) && !(objectType == typeof(double)))
		{
			return objectType == typeof(object);
		}
		return true;
	}
}
