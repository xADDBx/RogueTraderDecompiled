using System;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Newtonsoft.Json.Linq;
using RogueTrader.SharedTypes;
using UnityEngine;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

[Serializable]
public class BlueprintFieldOverrideOperation : BlueprintPatchOperation
{
	[ModsPatchSerializable]
	public object FieldValue;

	public Type FieldType => fieldType;

	public override void Apply(SimpleBlueprint bp)
	{
		base.Apply(bp);
		PFLog.Mods.Log($"Patching {TargetGuid} with override field {FieldName} value {FieldValue}");
		if (typeof(int).IsAssignableFrom(fieldType))
		{
			field.SetValue(fieldHolder, Convert.ToInt32(FieldValue));
			return;
		}
		if (fieldType.IsEnum)
		{
			field.SetValue(fieldHolder, Enum.ToObject(fieldType, FieldValue));
			return;
		}
		if (fieldType.IsSubclassOf(typeof(UnityEngine.Object)))
		{
			PFLog.Mods.Log($"Detected UnityEngine.Object derived field patching: {fieldType}");
			JObject obj = (JObject)FieldValue;
			string text = (string?)obj["guid"];
			long? num = (long?)obj["fileid"];
			PFLog.Mods.Log($"guid {text} fileid {num}");
			if (text == null || !num.HasValue)
			{
				PFLog.Mods.Error("Failed to parse UnityEngine.Object replace value: assetId is null or fileId null. Skipping patch...");
				return;
			}
			object obj2 = TryLoadGameResourceFromPatchFieldValue(text, num.Value);
			if (obj2 == null)
			{
				PFLog.Mods.Error($"Failed to load asset with assetId {text} fileId {num}. Skipping patch...");
				return;
			}
			PFLog.Mods.Log("Target Asset found, patching bp field value");
			field.SetValue(fieldHolder, obj2);
			return;
		}
		if (typeof(IReferenceBase).IsAssignableFrom(fieldType))
		{
			object obj3 = BlueprintPatchObjectComparator.TryFixFalseSerializedBlueprintReference(FieldValue, fieldType);
			if (obj3 != null)
			{
				field.SetValue(fieldHolder, obj3);
				return;
			}
		}
		try
		{
			field.SetValue(fieldHolder, FieldValue);
		}
		catch (Exception ex)
		{
			PFLog.Mods.Exception(ex, $"Got exception while trying to set value in blueprint patch. blueprint: {bp.name}, fieldName: {FieldName}, value: {FieldValue}");
		}
	}

	private object TryLoadGameResourceFromPatchFieldValue(string assetId, long fileId)
	{
		if ((bool)UnityObjectConverter.AssetList)
		{
			UnityEngine.Object @object = UnityObjectConverter.AssetList.Get(assetId, fileId);
			if (@object != null)
			{
				return @object;
			}
		}
		foreach (BlueprintReferencedAssets modificationAssetList in UnityObjectConverter.ModificationAssetLists)
		{
			UnityEngine.Object object2 = modificationAssetList.Get(assetId, fileId);
			if (object2 != null)
			{
				return object2;
			}
		}
		return null;
	}

	public override string ToString()
	{
		if (FieldValue != null)
		{
			return base.ToString() + " \n BlueprintFieldOverrideOperation " + FieldValue.ToString();
		}
		return base.ToString() + " \n BlueprintFieldOverrideOperation null";
	}
}
