using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using MemoryPack;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("0b3cc43201601904bb7eb333c9b646ff")]
[MemoryPackable(GenerateType.NoGenerate)]
[HashRoot]
public class SimpleBlueprint : IScriptableObjectWithAssetId, ICanBeLogContext
{
	[JsonIgnore]
	[NonOverridable]
	[InspectorReadOnly]
	public string name;

	[JsonIgnore]
	[NonOverridable]
	[InspectorReadOnly]
	public string AssetGuid;

	[SerializeField]
	[SerializeReference]
	[HideInInspector]
	[JsonIgnore]
	[SkipValidation]
	protected List<Element> m_AllElements;

	public List<Element> ElementsArray => m_AllElements ?? (m_AllElements = new List<Element>());

	string IScriptableObjectWithAssetId.name
	{
		get
		{
			return name;
		}
		set
		{
			name = value;
		}
	}

	string IScriptableObjectWithAssetId.AssetGuid
	{
		get
		{
			return AssetGuid;
		}
		set
		{
			AssetGuid = value;
		}
	}

	public void Become(SimpleBlueprint other)
	{
		JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(other), this);
	}

	[OnDeserializing]
	internal void OnDeserializing(StreamingContext context)
	{
		AssetGuid = Json.BlueprintBeingRead.AssetId;
		Json.BlueprintBeingRead.Data = this;
	}

	public virtual void OnValidate()
	{
	}

	public virtual void OnEnable()
	{
	}

	public int GetInstanceID()
	{
		return AssetGuid?.GetHashCode() ?? 0;
	}

	public static implicit operator bool(SimpleBlueprint o)
	{
		return o != null;
	}

	public static T Instantiate<T>(T obj) where T : UnityEngine.Object
	{
		return UnityEngine.Object.Instantiate(obj);
	}

	public void SetDirty()
	{
	}

	public virtual void Cleanup()
	{
	}

	public void AddToElementsList(Element e)
	{
		e.Owner = this;
		if (m_AllElements == null)
		{
			m_AllElements = new List<Element>();
		}
		m_AllElements.Add(e);
	}

	public void RemoveFromElementsList(Element element)
	{
		m_AllElements?.Remove(m_AllElements.FirstItem((Element e) => e?.name == element.name));
	}
}
