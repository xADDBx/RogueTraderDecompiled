using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Kingmaker.Networking.Serialization.Converters;

public class FloatAsIntConverter : JsonConverter
{
	public override bool CanConvert(Type objectType)
	{
		return typeof(float) == objectType;
	}

	public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
	{
		return BitConverter.Int32BitsToSingle(Convert.ToInt32(reader.Value, CultureInfo.InvariantCulture));
	}

	public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
	{
		int value2 = BitConverter.SingleToInt32Bits((float)value);
		writer.WriteValue(value2);
	}
}
