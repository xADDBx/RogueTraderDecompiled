using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("d04a8b4d1fec4545b9e4d90b81ce2498")]
public class HasFactGetter : PropertyGetter, PropertyContextAccessor.ITargetByType, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	[ValidateNotNull]
	private BlueprintUnitFactReference m_Fact;

	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!(this.GetTargetByType(Target) ?? throw new Exception($"HasFactGetter: can't find suitable target of type {Target}")).Facts.Contains((BlueprintUnitFact)m_Fact))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check if " + Target.Colorized() + " has " + m_Fact.NameSafe();
	}
}
