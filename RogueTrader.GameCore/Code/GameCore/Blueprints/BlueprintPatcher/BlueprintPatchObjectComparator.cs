using System;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Kingmaker.ElementsSystem;
using Kingmaker.Localization;
using Kingmaker.Utility.UnityExtensions;
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

	public static bool IsSimple(Type type)
	{
		if (!type.IsPrimitive && !type.Equals(typeof(string)))
		{
			return type.IsEnum;
		}
		return true;
	}

	public static bool ObjectsAreEqual(object protoItem, object targetItem, string fieldPath)
	{
		if (protoItem != null && targetItem != null && protoItem.GetType() != targetItem.GetType())
		{
			Logger.Error($"Possible bug: proto type {protoItem.GetType()} mismatch target type {targetItem.GetType()} at field {fieldPath}. If comparing element on RemoveItem operation, just check that types have the same base class.");
		}
		if (protoItem == null && targetItem == null)
		{
			return true;
		}
		if ((protoItem != null && IsSimple(protoItem.GetType())) || (targetItem != null && IsSimple(targetItem.GetType())))
		{
			Logger.Log($"proto {protoItem.GetType()} target {targetItem.GetType()}");
			Logger.Log("Blueprint patch inspection: parsing simple type field " + fieldPath);
			return targetItem == protoItem;
		}
		if (protoItem is BlueprintReferenceBase || targetItem is BlueprintReferenceBase)
		{
			Logger.Log("Blueprint patch inspection: parsing BlueprintReferenceBase derived type field " + fieldPath);
			BlueprintReferenceBase blueprintReferenceBase = protoItem as BlueprintReferenceBase;
			BlueprintReferenceBase blueprintReferenceBase2 = targetItem as BlueprintReferenceBase;
			string @this = blueprintReferenceBase?.Guid;
			string this2 = blueprintReferenceBase2?.Guid;
			if (blueprintReferenceBase == null || @this.IsNullOrEmpty())
			{
				Logger.Error("Item inside " + fieldPath + " is null. If the field is Array or List, then you should consider checking out the bp and probably remove the value.");
			}
			if (blueprintReferenceBase2 == null || this2.IsNullOrEmpty())
			{
				Logger.Error("Item inside " + fieldPath + " is null. If the field is Array or List, then you should consider checking out the bp and probably remove the value.");
			}
			return blueprintReferenceBase?.Equals(blueprintReferenceBase2) ?? blueprintReferenceBase2?.Equals(blueprintReferenceBase) ?? true;
		}
		if (protoItem is SimpleBlueprint || targetItem is SimpleBlueprint)
		{
			Logger.Log("Blueprint patch inspection: parsing SimpleBlueprint derived type field " + fieldPath);
			string obj = ((SimpleBlueprint)protoItem)?.AssetGuid;
			string text = ((SimpleBlueprint)targetItem)?.AssetGuid;
			return obj == text;
		}
		if (protoItem is Element || targetItem is Element)
		{
			Logger.Log("Blueprint patch inspection: parsing Element derived type field " + fieldPath);
			string obj2 = ((Element)protoItem)?.name;
			string text2 = ((Element)targetItem)?.name;
			return obj2 == text2;
		}
		if (protoItem is LocalizedString || targetItem is LocalizedString)
		{
			Logger.Log("Blueprint patch inspection: parsing LocalizedString derived type field " + fieldPath);
			LocalizedString obj3 = (LocalizedString)targetItem;
			LocalizedString localizedString = (LocalizedString)protoItem;
			string text3 = obj3?.Key;
			return localizedString?.Key == text3;
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
