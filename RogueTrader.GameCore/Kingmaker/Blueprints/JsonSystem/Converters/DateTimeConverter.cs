using System;
using Newtonsoft.Json;

namespace Kingmaker.Blueprints.JsonSystem.Converters;

public class DateTimeConverter : JsonConverter
{
	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		DateTimeOffset dateTimeOffset = (DateTimeOffset)value;
		writer.WriteValue((dateTimeOffset.Offset != TimeSpan.Zero) ? dateTimeOffset.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFzzz") : dateTimeOffset.UtcDateTime.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'FFFFFFFK"));
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		object value = reader.Value;
		DateTimeOffset dateTimeOffset = ((value is DateTime dateTime) ? new DateTimeOffset(dateTime) : ((!(value is string input)) ? DateTimeOffset.Now : DateTimeOffset.Parse(input)));
		return dateTimeOffset;
	}

	public override bool CanConvert(Type objectType)
	{
		return objectType == typeof(DateTimeOffset);
	}
}
