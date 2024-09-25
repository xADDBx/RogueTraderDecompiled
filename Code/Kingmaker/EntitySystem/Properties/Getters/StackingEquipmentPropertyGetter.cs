using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Buffs;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("07d839f88e7c4519b06213b456e0b567")]
public class StackingEquipmentPropertyGetter : PropertyGetter, PropertyContextAccessor.IMechanicContext, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintStackingUnitProperty.Reference m_Property;

	[SerializeField]
	private bool m_IsWeaponBased;

	public BlueprintStackingUnitProperty Property => m_Property?.Get();

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Stacked value of " + m_Property.NameSafe() + " on equipment of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		int num = (m_IsWeaponBased ? ((from p in base.PropertyContext.MechanicContext?.SourceAbilityContext?.Ability?.SourceItem?.Blueprint.GetComponents<StackingUnitProperty>()
			where p.Property == Property
			select p).Sum((StackingUnitProperty i) => GetValue(i, applyRanks: false))).GetValueOrDefault() : 0);
		EntityFactsManager facts = base.CurrentEntity.Facts;
		int num2 = 0;
		if (facts != null)
		{
			foreach (EntityFact item in facts.List)
			{
				if (item.MaybeContext == null)
				{
					continue;
				}
				foreach (StackingUnitProperty item2 in item.SelectComponents<StackingUnitProperty>())
				{
					if (item2.Property == Property)
					{
						num2 += item2.GetValue(item.MaybeContext);
					}
				}
				foreach (ContextStackingUnitProperty item3 in item.SelectComponents<ContextStackingUnitProperty>())
				{
					if (item3.Property == Property)
					{
						num2 += item3.PropertyValue.Calculate(item.MaybeContext);
					}
				}
			}
		}
		PartUnitBody bodyOptional = base.CurrentEntity.GetBodyOptional();
		if (bodyOptional == null)
		{
			return num2;
		}
		List<ItemSlot> list = ((!m_IsWeaponBased) ? new List<ItemSlot>
		{
			bodyOptional.Armor, bodyOptional.Belt, bodyOptional.Feet, bodyOptional.Gloves, bodyOptional.Glasses, bodyOptional.Head, bodyOptional.Neck, bodyOptional.Ring1, bodyOptional.Ring2, bodyOptional.Shirt,
			bodyOptional.Shoulders, bodyOptional.Wrist, bodyOptional.PrimaryHand, bodyOptional.SecondaryHand
		} : new List<ItemSlot>
		{
			bodyOptional.Armor, bodyOptional.Belt, bodyOptional.Feet, bodyOptional.Gloves, bodyOptional.Glasses, bodyOptional.Head, bodyOptional.Neck, bodyOptional.Ring1, bodyOptional.Ring2, bodyOptional.Shirt,
			bodyOptional.Shoulders, bodyOptional.Wrist
		});
		int num3 = 0;
		foreach (ItemSlot item4 in list)
		{
			BlueprintItem blueprintItem = item4.MaybeItem?.Blueprint;
			if (blueprintItem == null)
			{
				continue;
			}
			foreach (StackingUnitProperty component in blueprintItem.GetComponents<StackingUnitProperty>())
			{
				if (component.Property == Property)
				{
					num3 += GetValue(component, applyRanks: false);
				}
			}
		}
		return num2 + num3 + num;
	}

	private int GetValue(StackingUnitProperty p, bool applyRanks)
	{
		int num = ((!applyRanks) ? 1 : base.CurrentEntity.Buffs.Enumerable.Where((Buff i) => i.Blueprint == p.OwnerBlueprint).Sum((Buff i) => i.Rank));
		return p.GetValue(this.GetMechanicContext()) * num;
	}
}
