using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Converters;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.Blueprints;

[TypeId("220d3836215f96e4596d705f9c303c7d")]
public class PrototypeableObjectBase : ScriptableObject, ISerializationCallbackReceiver, IPrototypeableObjectBase
{
	[NonOverridable]
	[HideInInspector]
	[JsonConverter(typeof(PrototypeLinkConverter))]
	[JsonProperty(DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate)]
	public PrototypeableObjectBase PrototypeLink;

	public virtual void OnValidate()
	{
	}

	public virtual void OnEnable()
	{
	}

	public void OnBeforeSerialize()
	{
	}

	public void OnAfterDeserialize()
	{
	}
}
