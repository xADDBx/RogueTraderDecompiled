using Kingmaker.AreaLogic.Etudes;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.EntitySystem.Persistence.JsonUtility.Core;
using Kingmaker.Networking.Serialization.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Kingmaker.Networking.Serialization;

public static class GameStateJsonSerializer
{
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new GameStateContractResolver(),
		TypeNameHandling = TypeNameHandling.Auto,
		PreserveReferencesHandling = PreserveReferencesHandling.Objects,
		DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
		ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
		ReferenceResolverProvider = () => StrictReferenceResolver.Instance,
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
			(JsonConverter)new VersionConverter(),
			(JsonConverter)new EntityFactsManager.FactsGameStateAdapter(),
			(JsonConverter)new EntityPartsManager.PartsGameStateAdapter(),
			(JsonConverter)new EtudesSystem.EtudesDataGameStateAdapter()
		}
	};

	public static readonly JsonSerializer Serializer = JsonSerializer.Create(Settings);
}
