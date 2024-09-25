using System.IO;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;

namespace Kingmaker.Utility.NewtonsoftJson;

public static class NewtonsoftJsonHelper
{
	private static JsonSerializer Serializer => SaveSystemJsonSerializer.Serializer;

	public static T DeserializeFromFile<T>(string path)
	{
		using FileStream stream = File.OpenRead(path);
		return Deserialize<T>(new StreamReader(stream));
	}

	public static T Deserialize<T>(StreamReader stream)
	{
		using JsonTextReader reader = new JsonTextReader(stream);
		return Serializer.Deserialize<T>(reader);
	}
}
