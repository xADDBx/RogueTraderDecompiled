using System;

namespace Kingmaker.Blueprints;

[Serializable]
public class BlueprintReference<T> : BlueprintReferenceBase where T : BlueprintScriptableObject
{
	public T Get()
	{
		return GetBlueprint() as T;
	}

	public bool Is(BlueprintScriptableObject bp)
	{
		if (!bp)
		{
			return IsEmpty();
		}
		return guid == bp.AssetGuid;
	}

	public static TRef CreateTyped<TRef>(T bp) where TRef : BlueprintReference<T>, new()
	{
		return new TRef
		{
			guid = bp?.AssetGuid
		};
	}

	public static implicit operator T(BlueprintReference<T> reference)
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
		if (val == null)
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
