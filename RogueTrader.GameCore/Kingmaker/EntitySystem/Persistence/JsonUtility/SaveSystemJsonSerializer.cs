using System;
using Core.Async;
using Kingmaker.EntitySystem.Persistence.JsonUtility.Core;
using Kingmaker.Utility.CommandLineArgs;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public static class SaveSystemJsonSerializer
{
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new OptInContractResolver(),
		TypeNameHandling = TypeNameHandling.Auto,
		PreserveReferencesHandling = PreserveReferencesHandling.Objects,
		DefaultValueHandling = DefaultValueHandling.Include,
		ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
		ReferenceResolverProvider = ((Application.isEditor && SafeSaves) ? ((Func<IReferenceResolver>)(() => StrictReferenceResolver.Instance)) : null),
		Converters = 
		{
			(JsonConverter)new StringEnumConverter(),
			(JsonConverter)new Matrix4x4Converter(),
			(JsonConverter)new BlueprintConverter(),
			(JsonConverter)new DictionaryConverter(),
			(JsonConverter)new EntityRefConverter(),
			(JsonConverter)new CollectionsConverter(),
			(JsonConverter)new VersionConverter()
		},
		MaxDepth = 1024
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

	public static bool SafeSaves => CommandLineArguments.Parse().Contains("saves-support");
}
