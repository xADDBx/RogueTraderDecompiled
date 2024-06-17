using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.JsonUtility.Core;
using Kingmaker.Networking.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kingmaker.Networking.Serialization;

public static class NetSystemJsonSerializer
{
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new OptInContractResolver(),
		TypeNameHandling = TypeNameHandling.Auto,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
		ReferenceResolverProvider = null,
		Converters = 
		{
			(JsonConverter)new FloatAsIntConverter(),
			(JsonConverter)new StringEnumConverter(),
			(JsonConverter)new Vector3NullableConverter(),
			(JsonConverter)new Matrix4x4AsIntConverter(),
			(JsonConverter)new BlueprintConverter(),
			(JsonConverter)new DictionaryConverter(),
			(JsonConverter)new EntityRefConverter(),
			(JsonConverter)new CollectionsConverter(),
			(JsonConverter)new VersionConverter()
		}
	};

	public static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);
}
