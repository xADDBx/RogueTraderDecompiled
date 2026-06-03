using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items.Slots;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5237528c9da345589cf37a160a7d527a")]
public class CheckAbilityWeaponIsInPrimarySlot : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	private bool m_IsSecondarySlot;

	protected override int GetBaseValue()
	{
		if (!(this.GetAbilityWeapon()?.HoldingSlot is HandSlot handSlot) || handSlot.IsPrimaryHand != !m_IsSecondarySlot)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Weapon is in the " + (m_IsSecondarySlot ? "secondary" : "primary") + " hand slot";
	}
}
