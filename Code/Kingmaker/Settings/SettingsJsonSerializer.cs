using Core.Async;
using Newtonsoft.Json;

namespace Kingmaker.Settings;

public static class SettingsJsonSerializer
{
	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		Formatting = Formatting.None,
		Converters = { (JsonConverter)new JsonInt32AndSingleConverter() }
	};

	private static readonly JsonSerializer MainThreadSerializer = JsonSerializer.Create(Settings);

	public static JsonSerializer Serializer
	{
		get
		{
			if (!UnitySyncContextHolder.IsInUnity)
			{
				return JsonSerializer.Create(Settings);
			}
			return MainThreadSerializer;
		}
	}
}
