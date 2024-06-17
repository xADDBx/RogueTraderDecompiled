using System;
using System.Linq;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Utility;

public static class TransformExtention
{
	public static void ResetAll(this Transform transform)
	{
		transform.ResetPosition();
		transform.ResetRotation();
		transform.ResetScale();
	}

	public static void ResetPosition(this Transform transform)
	{
		transform.localPosition = Vector3.zero;
	}

	public static void ResetRotation(this Transform transform)
	{
		transform.localRotation = Quaternion.identity;
	}

	public static void ResetScale(this Transform transform)
	{
		transform.localScale = Vector3.one;
	}

	public static Transform FindRecursive(this Transform transform, string name)
	{
		return transform.FindRecursive((Transform t) => t.name == name);
	}

	public static Transform FindRecursive(this Transform transform, Func<Transform, bool> predicate)
	{
		if (transform.childCount == 0)
		{
			return null;
		}
		Transform transform2 = transform.Find(predicate);
		if ((bool)transform2)
		{
			return transform2;
		}
		foreach (Transform item in transform.Children())
		{
			Transform transform3 = item.FindRecursive(predicate);
			if ((bool)transform3)
			{
				return transform3;
			}
		}
		return null;
	}

	public static Transform Find(this Transform transform, Func<Transform, bool> predicate)
	{
		return transform.Children().FirstOrDefault(predicate);
	}

	public static string GetHierarchyPath(this Transform transform, bool addInstanceId = false, bool addGlobalObjectId = false)
	{
		return transform.name;
	}
}
