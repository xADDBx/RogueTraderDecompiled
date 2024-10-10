using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Kingmaker.Blueprints;

public static class BlueprintExtenstions
{
	public static void CallComponents<T>([CanBeNull] this BlueprintScriptableObject blueprint, Action<T> action)
	{
		if (blueprint == null)
		{
			return;
		}
		BlueprintComponent[] componentsArray = blueprint.ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			if (componentsArray[i] is T obj)
			{
				try
				{
					action(obj);
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
	}

	[CanBeNull]
	public static T GetComponent<T>([CanBeNull] this BlueprintScriptableObject blueprint)
	{
		if (blueprint == null)
		{
			return default(T);
		}
		for (int i = 0; i < blueprint.ComponentsArray.Length; i++)
		{
			BlueprintComponent blueprintComponent = blueprint.ComponentsArray[i];
			if (blueprintComponent is T)
			{
				return (T)(object)((blueprintComponent is T) ? blueprintComponent : null);
			}
		}
		return default(T);
	}

	public static bool TryGetComponent<T>([CanBeNull] this BlueprintScriptableObject blueprint, out T component)
	{
		component = default(T);
		if (blueprint == null)
		{
			return false;
		}
		for (int i = 0; i < blueprint.ComponentsArray.Length; i++)
		{
			if (blueprint.ComponentsArray[i] is T val)
			{
				component = val;
				return true;
			}
		}
		return false;
	}

	public static BlueprintComponentsEnumerator<T> GetComponents<T>([CanBeNull] this BlueprintScriptableObject blueprint)
	{
		return new BlueprintComponentsEnumerator<T>(blueprint);
	}

	public static string NameSafe(this ScriptableObject obj)
	{
		if (!obj)
		{
			return "<null>";
		}
		return obj.name;
	}

	public static T ReloadFromInstanceID<T>(this T obj) where T : UnityEngine.Object
	{
		return obj;
	}
}
