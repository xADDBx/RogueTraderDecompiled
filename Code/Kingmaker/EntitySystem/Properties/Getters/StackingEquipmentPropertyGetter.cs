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
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
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
		int num;
		if (!m_IsWeaponBased)
		{
			num = 0;
		}
		else
		{
			MechanicsContext mechanicContext = base.PropertyContext.MechanicContext;
			int? obj;
			if (mechanicContext == null)
			{
				obj = null;
			}
			else
			{
				AbilityExecutionContext sourceAbilityContext = mechanicContext.SourceAbilityContext;
				if (sourceAbilityContext == null)
				{
					obj = null;
				}
				else
				{
					AbilityData ability = sourceAbilityContext.Ability;
					if ((object)ability == null)
					{
						obj = null;
					}
					else
					{
						ItemEntity sourceItem = ability.SourceItem;
						obj = ((sourceItem != null) ? new int?((from p in sourceItem.Blueprint.GetComponents<StackingUnitProperty>()
							where p.Property == Property
							select p).Sum((StackingUnitProperty i) => GetValue(i, applyRanks: false))) : null);
					}
				}
			}
			int? num2 = obj;
			num = num2.GetValueOrDefault();
		}
		int num3 = num;
		EntityFactsManager facts = base.CurrentEntity.Facts;
		int num4 = 0;
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
						num4 += item2.GetValue(item.MaybeContext);
					}
				}
				foreach (ContextStackingUnitProperty item3 in item.SelectComponents<ContextStackingUnitProperty>())
				{
					if (item3.Property == Property)
					{
						num4 += item3.PropertyValue.Calculate(item.MaybeContext);
					}
				}
			}
		}
		PartUnitBody bodyOptional = base.CurrentEntity.GetBodyOptional();
		if (bodyOptional == null)
		{
			return num4;
		}
		List<ItemSlot> list = ((!m_IsWeaponBased) ? new List<ItemSlot>
		{
			bodyOptional.Armor, bodyOptional.Belt, bodyOptional.Feet, bodyOptional.Gloves, bodyOptional.Glasses, bodyOptional.Head, bodyOptional.Neck, bodyOptional.Ring1, bodyOptional.Ring2, bodyOptional.Shirt,
			bodyOptional.Shoulders, bodyOptional.Wrist, bodyOptional.PetProtocol, bodyOptional.PrimaryHand, bodyOptional.SecondaryHand
		} : new List<ItemSlot>
		{
			bodyOptional.Armor, bodyOptional.Belt, bodyOptional.Feet, bodyOptional.Gloves, bodyOptional.Glasses, bodyOptional.Head, bodyOptional.Neck, bodyOptional.Ring1, bodyOptional.Ring2, bodyOptional.Shirt,
			bodyOptional.Shoulders, bodyOptional.Wrist, bodyOptional.PetProtocol
		});
		int num5 = 0;
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
					num5 += GetValue(component, applyRanks: false);
				}
			}
		}
		return num4 + num5 + num3;
	}

	private int GetValue(StackingUnitProperty p, bool applyRanks)
	{
		int num = ((!applyRanks) ? 1 : base.CurrentEntity.Buffs.Enumerable.Where((Buff i) => i.Blueprint == p.OwnerBlueprint).Sum((Buff i) => i.Rank));
		return p.GetValue(this.GetMechanicContext()) * num;
	}
}
