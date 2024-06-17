using System;

namespace Kingmaker.ElementsSystem;

[Serializable]
public class ElementsReference<T> : ElementsReferenceBase where T : ElementsScriptableObject
{
	public T Get()
	{
		return GetObject() as T;
	}

	public bool Is(T bp)
	{
		if (!bp)
		{
			return IsEmpty();
		}
		return guid == bp.AssetGuid;
	}

	public static TRef CreateTyped<TRef>(T bp) where TRef : ElementsReference<T>, new()
	{
		return new TRef
		{
			guid = bp?.AssetGuid
		};
	}

	public static implicit operator T(ElementsReference<T> reference)
	{
		if (reference == null)
		{
			return null;
		}
		return reference.Get();
	}

	public string NameSafe()
	{
		T val = Get();
		if (!val)
		{
			if (!string.IsNullOrEmpty(guid))
			{
				return "NOT FOUND " + guid;
			}
			return "-not set-";
		}
		return val.name;
	}

	public override string ToString()
	{
		return guid;
	}
}
