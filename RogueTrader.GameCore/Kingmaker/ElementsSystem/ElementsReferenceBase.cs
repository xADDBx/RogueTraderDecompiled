using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Base;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.ElementsSystem;

[Serializable]
public class ElementsReferenceBase : IReferenceBase
{
	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	protected string guid;

	protected ElementsScriptableObject Cached { get; set; }

	public string Guid => guid;

	public ElementsScriptableObject GetObject()
	{
		if (!Cached)
		{
			Cached = ResourcesLibrary.TryGetScriptable(guid);
		}
		return Cached;
	}

	public bool IsEmpty()
	{
		if (!string.IsNullOrEmpty(guid))
		{
			return !GetObject();
		}
		return true;
	}

	public static TRef CreateTyped<TRef>(ElementsScriptableObject bp) where TRef : ElementsReferenceBase, new()
	{
		return new TRef
		{
			guid = bp?.AssetGuid
		};
	}

	public void ReadGuidFromJson(string guid)
	{
		this.guid = guid;
	}
}
