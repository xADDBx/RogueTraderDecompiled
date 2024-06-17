using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Owlcat.Runtime.Core.Utility;

public static class ObjectExtensions
{
	private class EnumerateChildrenList<T> : IEnumerable<T>, IEnumerable, IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly List<T> m_List;

		private int m_Idx;

		public static EnumerateChildrenList<T> Get { get; } = new EnumerateChildrenList<T>(new List<T>());


		public List<T> List => m_List;

		public T Current => m_List[m_Idx - 1];

		object IEnumerator.Current => Current;

		public EnumerateChildrenList(List<T> list)
		{
			m_List = list;
		}

		public void Dispose()
		{
			m_List.Clear();
			m_Idx = 0;
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this;
		}

		public bool MoveNext()
		{
			m_Idx++;
			return m_Idx <= m_List.Count;
		}

		public void Reset()
		{
			m_Idx = 0;
		}
	}

	public abstract class MaybeSelectRefType<T> where T : class
	{
	}

	public abstract class MaybeSelectValType<T> where T : struct
	{
	}

	public static T GetComponentNonAlloc<T>(this Component comp) where T : Component
	{
		if (!comp.TryGetComponent<T>(out var component))
		{
			return null;
		}
		return component;
	}

	public static T GetComponentNonAlloc<T>(this GameObject go) where T : class
	{
		if (!go.TryGetComponent<T>(out var component))
		{
			return null;
		}
		return component;
	}

	public static T EnsureComponent<T>(this Component comp) where T : Component
	{
		return comp.gameObject.EnsureComponent<T>();
	}

	public static T EnsureComponent<T>(this Component comp, out bool alreadyExists) where T : Component
	{
		return comp.gameObject.EnsureComponent<T>(out alreadyExists);
	}

	public static T EnsureComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponentNonAlloc<T>();
		if (val == null)
		{
			val = go.gameObject.AddComponent<T>();
		}
		return val;
	}

	public static T EnsureComponent<T>(this GameObject go, out bool alreadyExists) where T : Component
	{
		T val = go.GetComponentNonAlloc<T>();
		alreadyExists = val != null;
		if (val == null)
		{
			val = go.gameObject.AddComponent<T>();
		}
		return val;
	}

	public static IEnumerable<T> EnumerateComponentsInChildren<T>(this Component comp, bool includeInactive = false)
	{
		return comp.gameObject.EnumerateComponentsInChildren<T>(includeInactive);
	}

	public static IEnumerable<T> EnumerateComponentsInChildren<T>(this GameObject obj, bool includeInactive = false)
	{
		EnumerateChildrenList<T> get = EnumerateChildrenList<T>.Get;
		if (get.List.Count > 0)
		{
			throw new InvalidOperationException("Recursive EnumerateComponentsInChildren calls on same type not supported");
		}
		obj.GetComponentsInChildren(includeInactive, get.List);
		return get;
	}

	public static void ForAllChildren(this GameObject gameObject, Action<GameObject> action)
	{
		action(gameObject);
		foreach (Transform item in gameObject.transform)
		{
			item.gameObject.ForAllChildren(action);
		}
	}

	public static Transform FindChildRecursive(this Transform transform, string name)
	{
		try
		{
			if (transform.name.Equals(name, StringComparison.Ordinal))
			{
				return transform;
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform transform2 = transform.GetChild(i).FindChildRecursive(name);
				if (transform2 != null)
				{
					return transform2;
				}
			}
			return null;
		}
		finally
		{
		}
	}

	public static IEnumerable<Transform> Children(this Transform transform)
	{
		foreach (Transform item in transform)
		{
			yield return item;
		}
	}

	public static T Or<T>(this T unityObject, T valueIfNull) where T : UnityEngine.Object
	{
		if (unityObject != null)
		{
			return unityObject;
		}
		return valueIfNull;
	}

	public static TV MaybeSelect<T, TV>([CanBeNull] this T instance, Func<T, TV> selectFunc, MaybeSelectRefType<TV> hack = null) where T : class where TV : class
	{
		if (instance == null)
		{
			return null;
		}
		return selectFunc(instance);
	}

	public static TV? MaybeSelect<T, TV>([CanBeNull] this T instance, Func<T, TV> selectFunc, MaybeSelectValType<TV> hack = null) where T : class where TV : struct
	{
		if (instance == null)
		{
			return null;
		}
		return selectFunc(instance);
	}

	public static string GetHierarchyPath(this Transform current, string splitter)
	{
		if (current.parent == null)
		{
			return splitter + current.name;
		}
		return current.parent.GetHierarchyPath(splitter) + splitter + current.name;
	}

	public static Bounds GetRendererBoundingBox(this GameObject go)
	{
		Bounds bounds = default(Bounds);
		Renderer[] componentsInChildren = go.GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			if (bounds == default(Bounds))
			{
				bounds = renderer.bounds;
			}
			else
			{
				bounds.Encapsulate(renderer.bounds);
			}
		}
		return bounds;
	}
}
