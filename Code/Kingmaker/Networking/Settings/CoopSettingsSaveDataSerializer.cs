using Core.Async;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.JsonUtility.Core;
using Kingmaker.Networking.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kingmaker.Networking.Settings;

public static class CoopSettingsSaveDataSerializer
{
	private static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new OptInContractResolver(),
		TypeNameHandling = TypeNameHandling.Auto,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
		ReferenceResolverProvider = null,
		Converters = 
		{
			(JsonConverter)new StringEnumConverter(),
			(JsonConverter)new Vector3NullableConverter(),
			(JsonConverter)new Matrix4x4Converter(),
			(JsonConverter)new BlueprintConverter(),
			(JsonConverter)new DictionaryConverter(),
			(JsonConverter)new EntityRefConverter(),
			(JsonConverter)new CollectionsConverter(),
			(JsonConverter)new VersionConverter()
		}
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
