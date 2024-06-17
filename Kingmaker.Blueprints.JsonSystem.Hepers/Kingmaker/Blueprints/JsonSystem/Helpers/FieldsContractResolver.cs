using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Kingmaker.Blueprints.Hack;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.JsonSystem.Helpers;

public class FieldsContractResolver : DefaultContractResolver
{
	protected override List<MemberInfo> GetSerializableMembers(Type objectType)
	{
		if (!BuiltInUnityTypesSerializableMembers.TryGetMembers(objectType, out var result))
		{
			result = GetUnitySerializedFields(objectType).ToList();
		}
		if (objectType.IsSubclassOf(typeof(UnityEngine.Object)) && !objectType.GetInterfaces().Contains(typeof(IBlueprintScriptableObject)))
		{
			result.Add(objectType.GetProperty("name"));
		}
		return result;
	}

	public static IEnumerable<MemberInfo> GetUnitySerializedFields(Type type)
	{
		IEnumerable<MemberInfo> enumerable = null;
		if (type.BaseType != null)
		{
			enumerable = GetUnitySerializedFields(type.BaseType);
		}
		IEnumerable<FieldInfo> enumerable2 = from f in type.GetFields(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
			where f.IsPublic || f.HasAttribute<SerializeField>()
			where !f.HasAttribute<NonSerializedAttribute>()
			where IsSerializableType(f, f.FieldType)
			select f;
		return enumerable?.Concat(enumerable2) ?? enumerable2;
	}

	private static bool IsSerializableType(FieldInfo field, Type fieldType, bool arraysAllowed = true)
	{
		if (fieldType.IsPrimitive || fieldType.IsEnum || fieldType == typeof(string))
		{
			return true;
		}
		if (fieldType.IsOrSubclassOf<UnityEngine.Object>())
		{
			return true;
		}
		if (fieldType == typeof(object) && field.HasAttribute<ModsPatchSerializableAttribute>())
		{
			return true;
		}
		if (fieldType == typeof(AnimationCurve))
		{
			return true;
		}
		if (fieldType == typeof(Rect))
		{
			return true;
		}
		if (fieldType == typeof(Vector2))
		{
			return true;
		}
		if (fieldType == typeof(Vector3))
		{
			return true;
		}
		if (fieldType == typeof(Vector4))
		{
			return true;
		}
		if (fieldType == typeof(Vector2Int))
		{
			return true;
		}
		if (fieldType == typeof(Vector3Int))
		{
			return true;
		}
		if (fieldType == typeof(Color))
		{
			return true;
		}
		if (fieldType == typeof(Color32))
		{
			return true;
		}
		if (fieldType == typeof(Gradient))
		{
			return true;
		}
		if (fieldType.IsArray)
		{
			if (arraysAllowed)
			{
				return IsSerializableType(field, fieldType.GetElementType(), arraysAllowed: false);
			}
			return false;
		}
		if (fieldType.IsList())
		{
			if (arraysAllowed)
			{
				return IsSerializableType(field, fieldType.GetGenericArguments()[0], arraysAllowed: false);
			}
			return false;
		}
		if (fieldType.HasAttribute<SerializableAttribute>())
		{
			return fieldType.Assembly != typeof(int).Assembly;
		}
		return false;
	}

	protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
	{
		return base.CreateProperties(type, MemberSerialization.Fields);
	}

	protected override JsonContract CreateContract(Type objectType)
	{
		JsonContract jsonContract = base.CreateContract(objectType);
		if (objectType.IsValueType)
		{
			jsonContract.IsReference = false;
		}
		return jsonContract;
	}
}
