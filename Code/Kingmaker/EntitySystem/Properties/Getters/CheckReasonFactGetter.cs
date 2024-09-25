using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("02322c486992c2449ab0a19892384b33")]
public class CheckReasonFactGetter : PropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintUnitFactReference m_Blueprint;

	public BlueprintUnitFact Blueprint => m_Blueprint;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Context BP is {Blueprint}";
	}

	protected override int GetBaseValue()
	{
		if (base.PropertyContext.Rule?.Reason.Fact?.Blueprint != Blueprint)
		{
			return 0;
		}
		return 1;
	}
}
