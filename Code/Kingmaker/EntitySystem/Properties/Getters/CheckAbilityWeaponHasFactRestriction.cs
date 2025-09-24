using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("720e417ed95847439ae35dfa883ab9e1")]
public class CheckAbilityWeaponHasFactRestriction : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private BlueprintUnitFactReference m_RestrictionFact;

	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		if (abilityWeapon == null)
		{
			return 0;
		}
		BlueprintComponentsEnumerator<EquipmentRestrictionHasFacts> components = abilityWeapon.Blueprint.GetComponents<EquipmentRestrictionHasFacts>();
		if (components.Empty())
		{
			return 0;
		}
		if (!components.Any((EquipmentRestrictionHasFacts r) => r.Facts.Contains(m_RestrictionFact)))
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon is restricted by " + m_RestrictionFact?.Get()?.Name;
	}
}
