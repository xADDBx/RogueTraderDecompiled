using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Components;

[AllowedOn(typeof(BlueprintItemEquipment))]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("fb8ad0331874456ab1aaac562c259879")]
public class StackingUnitProperty : BlueprintComponent
{
	[SerializeField]
	private BlueprintStackingUnitProperty.Reference m_Property;

	[SerializeField]
	[Obsolete]
	private int PropertyValue;

	[SerializeField]
	private ContextValue m_Value;

	public BlueprintStackingUnitProperty Property => m_Property?.Get();

	public int GetValue(MechanicsContext context)
	{
		if (m_Value != null && !m_Value.IsZero)
		{
			return m_Value.Calculate(context);
		}
		return PropertyValue;
	}
}
