using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("c10cf729af04472a900528c00b86b0fd")]
public class BlueprintPropertyGetter : PropertyGetter
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintEntityPropertyReference m_Property;

	public PropertyCalculatorBlueprint Property => m_Property;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"#{Property}";
	}

	protected override int GetBaseValue()
	{
		return Property.GetValue(base.PropertyContext);
	}
}
