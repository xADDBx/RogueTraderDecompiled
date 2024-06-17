using System;
using System.Runtime.Serialization;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem;
using Kingmaker.Blueprints.JsonSystem.EditorDatabase;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[TypeId("0956112276c2f8340b645b372472e09a")]
[HashNoGenerate]
public class BlueprintComponent : ICanBeLogContext, IHavePrototype
{
	[Flags]
	private enum Flags
	{
		Disabled = 1
	}

	[Serializable]
	public class PrototypeLinkData
	{
		[JsonProperty(PropertyName = "guid")]
		public string BlueprintId;

		[JsonProperty(PropertyName = "name")]
		public string ComponentName;

		public BlueprintComponent GetComponent()
		{
			return BlueprintsDatabase.LoadById<BlueprintScriptableObject>(BlueprintId)?.ComponentsArray.FirstItem((BlueprintComponent c) => c.name == ComponentName);
		}
	}

	[HideInInspector]
	public string name;

	[SerializeField]
	[HideInInspector]
	[NonOverridable]
	private Flags m_Flags;

	[SerializeField]
	[NonOverridable]
	[JsonProperty(PropertyName = "PrototypeLink", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[HideInInspector]
	private PrototypeLinkData m_PrototypeLink;

	private bool False => false;

	public BlueprintScriptableObject OwnerBlueprint { get; set; }

	public bool Disabled
	{
		get
		{
			return (m_Flags & Flags.Disabled) != 0;
		}
		set
		{
			if (value)
			{
				m_Flags |= Flags.Disabled;
			}
			else
			{
				m_Flags &= ~Flags.Disabled;
			}
		}
	}

	public BlueprintComponent PrototypeLink
	{
		get
		{
			return m_PrototypeLink?.GetComponent();
		}
		set
		{
			m_PrototypeLink = ((value == null) ? null : new PrototypeLinkData
			{
				BlueprintId = value.OwnerBlueprint.AssetGuid,
				ComponentName = value.name
			});
		}
	}

	public static implicit operator bool(BlueprintComponent o)
	{
		return o != null;
	}

	public static T Instantiate<T>(T obj) where T : UnityEngine.Object
	{
		return UnityEngine.Object.Instantiate(obj);
	}

	[OnDeserialized]
	internal void OnDeserialized(StreamingContext context)
	{
		OwnerBlueprint = Json.BlueprintBeingRead.Data as BlueprintScriptableObject;
	}

	public override string ToString()
	{
		return GetType().Name;
	}
}
