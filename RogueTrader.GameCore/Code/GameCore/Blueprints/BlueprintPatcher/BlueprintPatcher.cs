using System.Collections.Generic;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Newtonsoft.Json;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

public static class BlueprintPatcher
{
	public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
	{
		ContractResolver = new FieldsContractResolver(),
		TypeNameHandling = TypeNameHandling.Auto,
		SerializationBinder = new GuidClassBinder(),
		Formatting = Formatting.Indented,
		PreserveReferencesHandling = PreserveReferencesHandling.None,
		DefaultValueHandling = DefaultValueHandling.Include,
		ReferenceLoopHandling = ReferenceLoopHandling.Error,
		Converters = 
		{
			(JsonConverter)new BlueprintReferenceConverter(),
			(JsonConverter)new StringEnumConverter(),
			(JsonConverter)new AnimationCurveConverter(),
			(JsonConverter)new GradientConverter(),
			(JsonConverter)new OverridesConverter(),
			(JsonConverter)new SharedStringConverter(),
			(JsonConverter)new UnityObjectConverter(),
			(JsonConverter)new Color32Converter()
		}
	};

	public static SimpleBlueprint TryPatchBlueprint(BlueprintPatch patch, SimpleBlueprint bp, string guid)
	{
		TryFixDeserialization(bp);
		if (patch != null && patch.TargetGuid == guid)
		{
			foreach (BlueprintPatchOperation patchingOperation in patch.PatchingOperations)
			{
				patchingOperation.Apply(bp);
			}
		}
		return bp;
	}

	private static void TryFixDeserialization(SimpleBlueprint bp)
	{
		if (Json.BlueprintBeingRead == null)
		{
			PFLog.Mods.Log($"Fixing Json.BeingRead for {bp}");
		}
		if (bp != null && Json.BlueprintBeingRead != null && Json.BlueprintBeingRead.Data != bp)
		{
			Json.BlueprintBeingRead = new BlueprintJsonWrapper(bp);
		}
	}

	public static object TryPatchBlueprint(List<BlueprintPatch> patches, SimpleBlueprint bp, string guid)
	{
		SimpleBlueprint simpleBlueprint = bp;
		foreach (BlueprintPatch patch in patches)
		{
			simpleBlueprint = TryPatchBlueprint(patch, simpleBlueprint, guid);
		}
		return simpleBlueprint;
	}
}
