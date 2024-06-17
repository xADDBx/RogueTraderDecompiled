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
[TypeId("bd3fcd4eeb5c4bc097136d82f6985e6c")]
public class ContextStackingUnitProperty : BlueprintComponent
{
	[SerializeField]
	private BlueprintStackingUnitProperty.Reference m_Property;

	public ContextValue PropertyValue;

	public BlueprintStackingUnitProperty Property => m_Property?.Get();
}
