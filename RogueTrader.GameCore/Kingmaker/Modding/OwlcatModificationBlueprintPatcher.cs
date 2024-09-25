using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Kingmaker.Modding;

public static class OwlcatModificationBlueprintPatcher
{
	private const int CacheSize = 1000;

	private const string PropertyNameEntries = "_#Entries";

	private const string PropertyNameArrayMergeSettings = "_#ArrayMergeSettings";

	private const string PropertyNameNullMergeSettings = "_#NullMergeSettings";

	private static readonly LinkedList<(SimpleBlueprint Blueprint, JObject SerializedObject)> JsonBlueprintsCache = new LinkedList<(SimpleBlueprint, JObject)>();

	private static readonly JsonMergeSettings MergeSettings = new JsonMergeSettings();

	[NotNull]
	private static JObject GetJObject(SimpleBlueprint blueprint)
	{
		foreach (var item in JsonBlueprintsCache)
		{
			if (item.Blueprint == blueprint)
			{
				return item.SerializedObject;
			}
		}
		if (JsonBlueprintsCache.Count >= 1000)
		{
			JsonBlueprintsCache.RemoveFirst();
		}
		BlueprintJsonWrapper value = new BlueprintJsonWrapper(blueprint)
		{
			AssetId = blueprint.AssetGuid.ToString()
		};
		JObject jObject;
		using (PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request())
		{
			using StringWriter textWriter = new StringWriter(pooledStringBuilder.Builder);
			Json.Serializer.Serialize(textWriter, value);
			jObject = JObject.Parse(pooledStringBuilder.Builder.ToString());
		}
		JsonBlueprintsCache.AddLast((blueprint, jObject));
		return jObject;
	}

	public static SimpleBlueprint ApplyPatch(SimpleBlueprint blueprint, string patchPath)
	{
		JObject patch = JObject.Parse(File.ReadAllText(patchPath));
		return ApplyPatch(blueprint, patch);
	}

	public static SimpleBlueprint ApplyPatch(SimpleBlueprint blueprint, JObject patch)
	{
		JObject jObject = GetJObject(blueprint);
		JObject jsonBlueprint = (JObject)jObject["Data"];
		if (patch["_#Entries"] is JArray jArray)
		{
			foreach (JToken item in jArray)
			{
				if (item is JObject patchEntry)
				{
					ApplyPatchEntry(jsonBlueprint, patchEntry);
				}
			}
		}
		else
		{
			ApplyPatchEntry(jsonBlueprint, patch);
		}
		using StringReader reader = new StringReader(jObject.ToString());
		using JsonTextReader reader2 = new JsonTextReader(reader);
		BlueprintJsonWrapper? blueprintJsonWrapper = Json.Serializer.Deserialize<BlueprintJsonWrapper>(reader2);
		blueprintJsonWrapper.Data.name = blueprint.name;
		blueprintJsonWrapper.Data.AssetGuid = blueprint.AssetGuid;
		return blueprintJsonWrapper.Data;
	}

	private static void ApplyPatchEntry(JObject jsonBlueprint, JObject patchEntry)
	{
		MergeSettings.MergeArrayHandling = ExtractMergeArraySettings(patchEntry);
		MergeSettings.MergeNullValueHandling = ExtractNullArraySettings(patchEntry);
		jsonBlueprint.Merge(patchEntry, MergeSettings);
	}

	private static MergeArrayHandling ExtractMergeArraySettings(JObject patchEntry)
	{
		string text = (string?)patchEntry["_#ArrayMergeSettings"];
		patchEntry.Remove("_#ArrayMergeSettings");
		return text switch
		{
			"Concat" => MergeArrayHandling.Concat, 
			"Union" => MergeArrayHandling.Union, 
			"Merge" => MergeArrayHandling.Merge, 
			_ => MergeArrayHandling.Replace, 
		};
	}

	private static MergeNullValueHandling ExtractNullArraySettings(JObject patchEntry)
	{
		string? text = (string?)patchEntry["_#NullMergeSettings"];
		patchEntry.Remove("_#NullMergeSettings");
		if (text == "Merge")
		{
			return MergeNullValueHandling.Merge;
		}
		return MergeNullValueHandling.Ignore;
	}
}
