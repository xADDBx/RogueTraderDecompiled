using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using MemoryPack;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[HashRoot]
[TypeId("3ec4f91d40b87d34197f44f40a969d92")]
[MemoryPackable(GenerateType.NoGenerate)]
public class BlueprintScriptableObject : SimpleBlueprint, IHavePrototype
{
	[NonOverridable]
	[SerializeField]
	[JsonProperty(PropertyName = "PrototypeLink")]
	[HideInInspector]
	private string m_PrototypeId;

	[NonOverridable]
	[SerializeField]
	[JsonProperty(PropertyName = "m_Overrides", DefaultValueHandling = DefaultValueHandling.Ignore)]
	[HideInInspector]
	private List<string> m_Overrides = new List<string>();

	[NonOverridable]
	[SkipValidation]
	[HideInInspector]
	[SerializeField]
	[SerializeReference]
	private BlueprintComponent[] Components;

	[SerializeField]
	[TextArea(5, 6)]
	public string Comment;

	public BlueprintComponent[] ComponentsArray
	{
		get
		{
			return Components ?? (Components = Array.Empty<BlueprintComponent>());
		}
		set
		{
			Components = value ?? Array.Empty<BlueprintComponent>();
		}
	}

	public string AssetGuidThreadSafe => AssetGuid;

	public virtual bool AllowContextActionsOnly => false;

	public IHavePrototype PrototypeLink => ResourcesLibrary.TryGetBlueprint(m_PrototypeId) as BlueprintScriptableObject;

	public override string ToString()
	{
		return name;
	}

	public override void OnValidate()
	{
		if (BlueprintValidationHelper.AllowOnValidate)
		{
			base.OnValidate();
		}
	}

	public virtual void OnEnableWithLibrary()
	{
	}

	public override void OnEnable()
	{
		base.OnEnable();
		BlueprintComponent[] componentsArray = ComponentsArray;
		for (int i = 0; i < componentsArray.Length; i++)
		{
			componentsArray[i].OwnerBlueprint = this;
		}
		foreach (Element item in base.ElementsArray)
		{
			if (item != null)
			{
				item.Owner = this;
			}
			else
			{
				LogChannel.System.Warning($"BlueprintScriptableObject.OnEnable: blueprint has null in ElementsArray ({this})");
			}
		}
		OnEnableWithLibrary();
	}
}
