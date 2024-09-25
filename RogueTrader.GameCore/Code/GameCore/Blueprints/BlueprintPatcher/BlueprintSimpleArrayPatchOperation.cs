using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using UnityEngine;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

[Serializable]
public class BlueprintSimpleArrayPatchOperation : BlueprintPatchOperation
{
	[SerializeField]
	public BlueprintPatchOperationType OperationType;

	[ModsPatchSerializable]
	public object Value;

	[ModsPatchSerializable]
	public object TargetValue;

	private Type GetArrayElementType(FieldInfo fieldInfo)
	{
		Type listElementType = BlueprintPatchOperation.GetListElementType(fieldInfo.FieldType);
		if (listElementType == null)
		{
			throw new Exception($"Failed to get ElementType for {concreteBlueprintType} {fieldInfo.Name}");
		}
		return listElementType;
	}

	public override void Apply(SimpleBlueprint bp)
	{
		PFLog.Mods.Log("Patching array " + FieldName + " of " + TargetGuid);
		base.Apply(bp);
		switch (OperationType)
		{
		case BlueprintPatchOperationType.InsertAfterElement:
		case BlueprintPatchOperationType.InsertBeforeElement:
		case BlueprintPatchOperationType.InsertAtBeginning:
		case BlueprintPatchOperationType.InsertLast:
			InsertElement();
			break;
		case BlueprintPatchOperationType.RemoveElement:
			RemoveElement();
			break;
		default:
			PFLog.Mods.Error($"Current patch operation type is unsupported {OperationType}");
			break;
		}
	}

	private void OverrideArray()
	{
		field.SetValue(fieldHolder, Value);
	}

	private void UnionArrays()
	{
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = (IList)Value;
		IList list3 = MaybeToList(Array.CreateInstance(arrayElementType, list.Count + list2.Count), this);
		for (int i = 0; i < list.Count; i++)
		{
			list3[i] = list[i];
		}
		for (int j = 0; j < list2.Count; j++)
		{
			list3[j] = list2[j];
		}
		field.SetValue(fieldHolder, list3);
	}

	private void UnionDistinctArrays()
	{
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = (IList)Value;
		IList list3 = MaybeToList(Array.CreateInstance(arrayElementType, list.Count + list2.Count), this);
		for (int i = 0; i < list.Count; i++)
		{
			list3[i] = list[i];
		}
		for (int j = 0; j < list2.Count; j++)
		{
			if (!list3.Contains(list2[j]))
			{
				list3[j] = list2[j];
			}
		}
		field.SetValue(fieldHolder, list3);
	}

	private void UnionList()
	{
		IList list = (IList)field.GetValue(fieldHolder);
		foreach (object item in (IList)Value)
		{
			list.Add(item);
		}
		field.SetValue(fieldHolder, list);
	}

	private void UnionDistinctList()
	{
		IList list = (IList)field.GetValue(fieldHolder);
		foreach (object item in (IList)Value)
		{
			if (list.Contains(item))
			{
				list.Add(item);
			}
		}
		field.SetValue(fieldHolder, list);
	}

	private void ReplaceElement()
	{
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = MaybeToList(Array.CreateInstance(arrayElementType, list.Count), this);
		Type type = Value.GetType();
		PFLog.Mods.Log($"Raw Value type : {type}");
		PFLog.Mods.Log($"List element type : {arrayElementType}");
		object obj = Value;
		if (obj.GetType() != arrayElementType && !arrayElementType.IsAssignableFrom(type) && !type.IsAssignableFrom(arrayElementType))
		{
			PFLog.Mods.Log("Patch value and field element type mismatch. Try to treat the field like BlueprintReference.");
			obj = BlueprintPatchObjectComparator.TryFixFalseSerializedBlueprintReference(Value, arrayElementType);
			if (obj == null)
			{
				PFLog.Mods.Error("Unable to fix " + FieldName + " patch value.");
				return;
			}
			PFLog.Mods.Log("Value fixed as BlueprintReference.");
		}
		for (int i = 0; i < list.Count; i++)
		{
			object obj2 = list[i];
			if (BlueprintPatchObjectComparator.ObjectsAreEqual(TargetValue, obj2, FieldName))
			{
				obj2 = obj;
			}
			list2[i] = obj2;
		}
		field.SetValue(fieldHolder, list2);
	}

	private static IList MaybeToList(IList array, BlueprintSimpleArrayPatchOperation patchOp)
	{
		if (!patchOp.CheckTypeIsList(patchOp.fieldType))
		{
			return array;
		}
		Type elementType = array.GetType().GetElementType();
		Type[] typeArguments = new Type[1] { elementType };
		object[] parameters = new object[1] { array };
		return (IList)typeof(Enumerable).GetMethod("ToList", BindingFlags.Static | BindingFlags.Public).MakeGenericMethod(typeArguments).Invoke(null, parameters);
	}

	private void RemoveElement()
	{
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = MaybeToList(Array.CreateInstance(arrayElementType, list.Count - 1), this);
		Type type = Value.GetType();
		PFLog.Mods.Log($"Raw Value type : {type}");
		PFLog.Mods.Log($"List element type : {arrayElementType}");
		object obj = Value;
		if (obj.GetType() != arrayElementType && !arrayElementType.IsAssignableFrom(type) && !type.IsAssignableFrom(arrayElementType))
		{
			PFLog.Mods.Log("Patch value and field element type mismatch. Try to treat the field like BlueprintReference.");
			obj = BlueprintPatchObjectComparator.TryFixFalseSerializedBlueprintReference(Value, arrayElementType);
			if (obj == null)
			{
				PFLog.Mods.Error("Unable to fix " + FieldName + " patch value.");
				return;
			}
			PFLog.Mods.Log("Value fixed as BlueprintReference.");
		}
		int num = -1;
		for (int i = 0; i < list.Count; i++)
		{
			if (BlueprintPatchObjectComparator.ObjectsAreEqual(list[i], obj, FieldName))
			{
				num = i;
				break;
			}
			if (i >= list2.Count)
			{
				PFLog.Mods.Error("Item was not found in target list, and there's danger of IndexOutOfRange");
				break;
			}
			list2[i] = list[i];
		}
		if (num == -1)
		{
			PFLog.Mods.Error("No item to remove found in target list field " + FieldName);
			return;
		}
		for (int j = num + 1; j < list.Count; j++)
		{
			list2[j - 1] = list[j];
		}
		field.SetValue(fieldHolder, list2);
	}

	private void InsertElement()
	{
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = MaybeToList(Array.CreateInstance(arrayElementType, list.Count + 1), this);
		int num = CalculateReplaceIndex(list);
		if (num == -1)
		{
			PFLog.Mods.Error("Couldn't find index in array " + field.Name + " by given target value. Aborting patch...");
			return;
		}
		for (int i = 0; i < num; i++)
		{
			list2[i] = list[i];
		}
		Type type = Value.GetType();
		PFLog.Mods.Log($"Raw Value type : {type}");
		PFLog.Mods.Log($"element type : {arrayElementType}");
		object obj = Value;
		if (obj.GetType() != arrayElementType && !arrayElementType.IsAssignableFrom(type) && !type.IsAssignableFrom(arrayElementType))
		{
			PFLog.Mods.Log("Patch value and field element type mismatch. Try to treat the field like BlueprintReference.");
			obj = BlueprintPatchObjectComparator.TryFixFalseSerializedBlueprintReference(Value, arrayElementType);
			if (obj == null)
			{
				PFLog.Mods.Error("Unable to fix " + FieldName + " patch value.");
				return;
			}
			PFLog.Mods.Log("Value fixed as BlueprintReference.");
		}
		list2[num] = obj;
		for (int j = num; j < list.Count; j++)
		{
			list2[j + 1] = list[j];
		}
		field.SetValue(fieldHolder, list2);
	}

	private int CalculateReplaceIndex(IList array)
	{
		int num = -1;
		switch (OperationType)
		{
		case BlueprintPatchOperationType.InsertAtBeginning:
			return 0;
		case BlueprintPatchOperationType.InsertLast:
			return array.Count;
		case BlueprintPatchOperationType.InsertAfterElement:
		case BlueprintPatchOperationType.InsertBeforeElement:
		{
			for (int i = 0; i < array.Count; i++)
			{
				if (BlueprintPatchObjectComparator.ObjectsAreEqual(array[i], TargetValue, FieldName))
				{
					num = ((OperationType == BlueprintPatchOperationType.InsertAfterElement) ? (i + 1) : i);
					break;
				}
			}
			if (num == -1)
			{
				PFLog.Mods.Error("Failed to calculate insert index, target item not found for field " + FieldName);
			}
			return num;
		}
		default:
			PFLog.Mods.Error($"Unsupported BlueprintPatchOperationType given while calculating Insert Index {OperationType}");
			return -1;
		}
	}

	public override string ToString()
	{
		return $"{base.ToString()} \n BlueprintSimpleArrayPatchOperation: type {OperationType.ToString()} \n value {TargetValue}";
	}
}
