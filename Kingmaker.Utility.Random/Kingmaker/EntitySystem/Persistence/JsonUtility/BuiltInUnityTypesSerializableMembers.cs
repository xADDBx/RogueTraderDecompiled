using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Kingmaker.EntitySystem.Persistence.JsonUtility;

public static class BuiltInUnityTypesSerializableMembers
{
	private static Dictionary<Type, List<string>> SerializableMembers = new Dictionary<Type, List<string>>
	{
		{
			typeof(Vector4),
			new List<string> { "x", "y", "z", "w" }
		},
		{
			typeof(Vector3),
			new List<string> { "x", "y", "z" }
		},
		{
			typeof(Vector3Int),
			new List<string> { "x", "y", "z" }
		},
		{
			typeof(Vector2),
			new List<string> { "x", "y" }
		},
		{
			typeof(Vector2Int),
			new List<string> { "x", "y" }
		},
		{
			typeof(Quaternion),
			new List<string> { "x", "y", "z", "w" }
		},
		{
			typeof(Bounds),
			new List<string> { "center", "extents" }
		},
		{
			typeof(Color),
			new List<string> { "r", "g", "b", "a" }
		},
		{
			typeof(Rect),
			new List<string> { "x", "y", "width", "height" }
		}
	};

	public static bool TryGetMembers(Type type, out List<MemberInfo> result)
	{
		if (SerializableMembers.TryGetValue(type, out var value))
		{
			result = new List<MemberInfo>(value.Count);
			foreach (string item in value)
			{
				MemberInfo[] member = type.GetMember(item);
				if (member.Length != 1)
				{
					throw new Exception($"Incorrect member name '{item}' for type '{type}'");
				}
				result.Add(member[0]);
			}
			return true;
		}
		result = null;
		return false;
	}
}
