using System;
using System.Collections;
using System.Reflection;
using Kingmaker;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Code.GameCore.Blueprints.BlueprintPatcher;

[Serializable]
public class BlueprintSimpleArrayPatchOperation : BlueprintPatchOperation
{
	[SerializeField]
	public BlueprintPatchOperationType OperationType;

	[ModsPatchSerializable]
	public object Value;

	[SerializeField]
	public string TargetValueGuid;

	[ModsPatchSerializable]
	public object TargetValue;

	private Type GetArrayElementType(FieldInfo fieldInfo)
	{
		Type elementType = fieldInfo.FieldType.GetElementType();
		if (elementType == null)
		{
			throw new Exception($"Failed to get ElementType for {concreteBlueprintType} {fieldInfo.Name}");
		}
		return elementType;
	}

	public override void Apply(SimpleBlueprint bp)
	{
		if (OperationType == BlueprintPatchOperationType.Undefined)
		{
			throw new Exception("Corrupted patch. Undefined operation type is given.");
		}
		PFLog.Mods.Log("Patching array " + FieldName + " of " + TargetGuid);
		base.Apply(bp);
		bool flag = CheckTypeIsArrayOrListOfBlueprintReferences(fieldType);
		if (flag && OperationType == BlueprintPatchOperationType.ReplaceElement)
		{
			ReplaceElement();
		}
		else if (flag && OperationType == BlueprintPatchOperationType.RemoveElement)
		{
			RemoveElement();
		}
		else if (flag && (OperationType == BlueprintPatchOperationType.InsertLast || OperationType == BlueprintPatchOperationType.InsertAfterElement || OperationType == BlueprintPatchOperationType.InsertBeforeElement || OperationType == BlueprintPatchOperationType.InsertAtBeginning))
		{
			InsertElement();
		}
		else if (CheckTypeIsList(fieldType) && OperationType == BlueprintPatchOperationType.Override)
		{
			OverrideArray();
		}
		else if (CheckTypeIsList(fieldType) && OperationType == BlueprintPatchOperationType.Union)
		{
			UnionList();
		}
		else if (CheckTypeIsList(fieldType) && OperationType == BlueprintPatchOperationType.UnionDistinct)
		{
			UnionDistinctList();
		}
		else if (fieldType.IsArray && OperationType == BlueprintPatchOperationType.Override)
		{
			OverrideArray();
		}
		else if (fieldType.IsArray && OperationType == BlueprintPatchOperationType.Union)
		{
			UnionArrays();
		}
		else if (fieldType.IsArray && OperationType == BlueprintPatchOperationType.UnionDistinct)
		{
			UnionDistinctArrays();
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
		IList list3 = Array.CreateInstance(arrayElementType, list.Count + list2.Count);
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
		IList list3 = Array.CreateInstance(arrayElementType, list.Count + list2.Count);
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
		if (TargetValueGuid.IsNullOrEmpty())
		{
			throw new Exception("Null target value guid given in patch operation");
		}
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = Array.CreateInstance(arrayElementType, list.Count);
		for (int i = 0; i < list.Count; i++)
		{
			object obj = list[i];
			BlueprintReferenceBase blueprintReferenceBase = (BlueprintReferenceBase)obj;
			if (blueprintReferenceBase != null && blueprintReferenceBase.Guid == TargetValueGuid)
			{
				obj = ((JObject)Value).ToObject(arrayElementType);
			}
			list2[i] = obj;
		}
		field.SetValue(fieldHolder, list2);
	}

	private void RemoveElement()
	{
		if (TargetValueGuid.IsNullOrEmpty())
		{
			throw new Exception("Null target value guid given in patch operation");
		}
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = Array.CreateInstance(arrayElementType, list.Count - 1);
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			object obj = list[i];
			BlueprintReferenceBase blueprintReferenceBase = (BlueprintReferenceBase)obj;
			if (blueprintReferenceBase != null)
			{
				if (!(blueprintReferenceBase.Guid != TargetValueGuid))
				{
					num = i;
					break;
				}
				list2[i] = obj;
			}
		}
		for (int j = num; j < list.Count; j++)
		{
			list2[j - 1] = list[j];
		}
		field.SetValue(fieldHolder, list2);
	}

	private void InsertElement()
	{
		Type arrayElementType = GetArrayElementType(field);
		IList list = (IList)field.GetValue(fieldHolder);
		IList list2 = Array.CreateInstance(arrayElementType, list.Count + 1);
		int num = CalculateReplaceIndex(list);
		if (num == -1)
		{
			throw new Exception("Couldn't find index in array " + field.Name + " by given target value");
		}
		for (int i = 0; i < num; i++)
		{
			list2[i] = list[i];
		}
		object value = ((JObject)Value).ToObject(arrayElementType);
		list2[num] = value;
		for (int j = num; j < list.Count; j++)
		{
			list2[j + 1] = list[j];
		}
		field.SetValue(fieldHolder, list2);
	}

	private int CalculateReplaceIndex(IList array)
	{
		if (OperationType == BlueprintPatchOperationType.Override || OperationType == BlueprintPatchOperationType.Union || OperationType == BlueprintPatchOperationType.UnionDistinct)
		{
			throw new Exception($"Replace index cannot be calculated for operation type {OperationType}");
		}
		if (array.Count == 0)
		{
			return -1;
		}
		int result = 0;
		if (OperationType == BlueprintPatchOperationType.InsertAtBeginning)
		{
			result = 0;
		}
		if (OperationType == BlueprintPatchOperationType.InsertLast)
		{
			result = array.Count;
		}
		if (OperationType == BlueprintPatchOperationType.InsertAfterElement)
		{
			if (typeof(BlueprintReferenceBase).IsAssignableFrom(array[0].GetType()))
			{
				for (int i = 0; i < array.Count; i++)
				{
					if (!(((BlueprintReferenceBase)array[i]).Guid != TargetValueGuid))
					{
						result = i + 1;
						break;
					}
				}
			}
			else
			{
				for (int j = 0; j < array.Count; j++)
				{
					if (array[j] != TargetValue)
					{
						result = j + 1;
						break;
					}
				}
			}
		}
		if (OperationType == BlueprintPatchOperationType.InsertBeforeElement)
		{
			if (typeof(BlueprintReferenceBase).IsAssignableFrom(array[0].GetType()))
			{
				for (int k = 0; k < array.Count; k++)
				{
					if (!(((BlueprintReferenceBase)array[k]).Guid != TargetValueGuid))
					{
						result = k;
						break;
					}
				}
			}
			else
			{
				for (int l = 0; l < array.Count; l++)
				{
					if (array[l] != TargetValue)
					{
						result = l;
						break;
					}
				}
			}
		}
		return result;
	}

	public override string ToString()
	{
		return $"{base.ToString()} \n BlueprintSimpleArrayPatchOperation: type {OperationType.ToString()} \n value {TargetValue}";
	}
}
