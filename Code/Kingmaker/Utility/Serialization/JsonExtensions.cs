using System;
using System.Globalization;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace Kingmaker.Utility.Serialization;

public static class JsonExtensions
{
	public static T DeserializeObject<T>(this JsonSerializer serializer, string source)
	{
		using StringReader reader = new StringReader(source);
		using JsonTextReader reader2 = new JsonTextReader(reader);
		return serializer.Deserialize<T>(reader2);
	}

	public static T DeserializeObject<T>(this JsonSerializer serializer, Stream source)
	{
		using StreamReader reader = new StreamReader(source);
		return (T)serializer.Deserialize(reader, typeof(T));
	}

	public static string SerializeObject<T>(this JsonSerializer serializer, T source)
	{
		using StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		serializer.Serialize(stringWriter, source);
		return stringWriter.ToString();
	}

	public static string SerializeObject<T>(this JsonSerializer serializer, T source, StringWriter writer, bool clear = true)
	{
		serializer.Serialize(writer, source);
		string result = writer.ToString();
		if (clear)
		{
			writer.GetStringBuilder().Clear();
		}
		return result;
	}

	public static void SerializeObject<T>(this JsonSerializer serializer, T source, Stream stream)
	{
		using StreamWriter textWriter = new StreamWriter(stream);
		serializer.Serialize(textWriter, source);
	}

	public static void SerializeToFile(this JsonSerializer jsonSerializer, string path, object data)
	{
		using StreamWriter textWriter = File.CreateText(path);
		jsonSerializer.Serialize(textWriter, data);
	}

	public static int SerializeToBson<T>(this JsonSerializer serializer, T source, byte[] buffer, int startIndex)
	{
		using MemoryStream memoryStream = new MemoryStream();
		using BsonWriter jsonWriter = new BsonWriter(memoryStream);
		serializer.Serialize(jsonWriter, source);
		memoryStream.Seek(0L, SeekOrigin.Begin);
		int num = (int)memoryStream.Length;
		int num2 = memoryStream.Read(buffer, startIndex, num);
		if (num2 != num)
		{
			throw new Exception($"SerializeToBson: n ({num2}) != count ({num})");
		}
		return num2;
	}

	public static T DeserializeFromBson<T>(this JsonSerializer serializer, byte[] data, int index, int length)
	{
		using MemoryStream stream = new MemoryStream(data, index, length);
		using BsonReader reader = new BsonReader(stream);
		return serializer.Deserialize<T>(reader);
	}
}
