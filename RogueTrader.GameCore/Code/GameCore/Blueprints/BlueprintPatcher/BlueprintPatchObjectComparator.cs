using System;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

public static class BlueprintPatchObjectComparator
{
	private static readonly LogChannel Logger = PFLog.Mods;

	public static object TryFixFalseSerializedBlueprintReference(object value, Type targetType)
	{
		if (value == null)
		{
			return null;
		}
		if (value.GetType() == typeof(string) && typeof(IReferenceBase).IsAssignableFrom(targetType))
		{
			string text = (string)value;
			if (!text.StartsWith("!bp_"))
			{
				return null;
			}
			try
			{
				object obj = Activator.CreateInstance(targetType);
				string value2 = text.Substring(4);
				if (obj is IReferenceBase referenceBase)
				{
					referenceBase.ReadGuidFromJson(value2);
				}
				return obj;
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
				return null;
			}
		}
		return null;
	}

	public static bool IsSimple(object value)
	{
		Type type = value.GetType();
		if (type.IsPrimitive || type.IsEnum)
		{
			return true;
		}
		if (type.Equals(typeof(string)))
		{
			if (((string)value).StartsWith("!bp_"))
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public static bool ObjectsAreEqual(object protoItem, object targetItem, string fieldPath)
	{
		Logger.Log($"Proto : {protoItem}");
		Logger.Log($"Target : {targetItem}");
		if (protoItem != null && targetItem != null && protoItem.GetType() != targetItem.GetType())
		{
			Logger.Error($"Possible bug: proto type {protoItem.GetType()} mismatch target type {targetItem.GetType()} at field {fieldPath}. If comparing element on RemoveItem operation, just check that types have the same base class.");
		}
		if (protoItem == null && targetItem == null)
		{
			return true;
		}
		if ((protoItem != null && IsSimple(protoItem)) || (targetItem != null && IsSimple(targetItem)))
		{
			Logger.Log("Blueprint patch inspection: parsing simple type field " + fieldPath);
			Logger.Log($"proto {protoItem.GetType()} target {targetItem.GetType()}");
			return targetItem == protoItem;
		}
		if (protoItem is BlueprintReferenceBase || targetItem is BlueprintReferenceBase)
		{
			Logger.Log("Blueprint patch inspection: parsing BlueprintReferenceBase derived type field " + fieldPath);
			BlueprintReferenceBase blueprintReferenceBase = protoItem as BlueprintReferenceBase;
			object obj = Activator.CreateInstance(blueprintReferenceBase.GetType());
			string text = (targetItem as string) ?? targetItem?.ToString();
			if (text != null && text.StartsWith("!bp_"))
			{
				text = text.Substring(4);
				if (obj is IReferenceBase referenceBase)
				{
					referenceBase.ReadGuidFromJson(text);
				}
			}
			if (obj == null)
			{
				Logger.Error("Failed to cast to BlueprintReferenceBase");
				return false;
			}
			BlueprintReferenceBase blueprintReferenceBase2 = (BlueprintReferenceBase)obj;
			return blueprintReferenceBase.Guid == blueprintReferenceBase2.Guid;
		}
		if (protoItem is SimpleBlueprint || targetItem is SimpleBlueprint)
		{
			Logger.Log("Blueprint patch inspection: parsing SimpleBlueprint derived type field " + fieldPath);
			string obj2 = ((SimpleBlueprint)protoItem)?.AssetGuid;
			string text2 = ((SimpleBlueprint)targetItem)?.AssetGuid;
			return obj2 == text2;
		}
		if (protoItem is Element || targetItem is Element)
		{
			Logger.Log("Blueprint patch inspection: parsing Element derived type field " + fieldPath);
			string obj3 = ((Element)protoItem)?.name;
			string text3 = ((Element)targetItem)?.name;
			return obj3 == text3;
		}
		if (protoItem is LocalizedString || targetItem is LocalizedString)
		{
			Logger.Log("Blueprint patch inspection: parsing LocalizedString derived type field " + fieldPath);
			LocalizedString obj4 = (LocalizedString)targetItem;
			LocalizedString localizedString = (LocalizedString)protoItem;
			string text4 = obj4?.Key;
			return localizedString?.Key == text4;
		}
		if (protoItem is UnityEngine.Object || targetItem is UnityEngine.Object)
		{
			Logger.Log("Blueprint patch inspection: parsing GameObject derived type field " + fieldPath);
			if (protoItem == targetItem)
			{
				return true;
			}
			if (targetItem != null)
			{
				int instanceID = ((UnityEngine.Object)targetItem).GetInstanceID();
				if (instanceID == 0)
				{
					Logger.Log("Target value for " + fieldPath + " is not set GameObject");
					targetItem = null;
				}
				if (protoItem == null && instanceID == 0)
				{
					Logger.Log("Both values are GameObject's null in " + fieldPath + ".");
					return true;
				}
			}
			return false;
		}
		Logger.Error($"Blueprint patch inspection : Parsing unhandled item type {targetItem.GetType()} while collection patch creation. Comparison is probably unreliable for field {fieldPath}");
		return protoItem == targetItem;
	}
}
