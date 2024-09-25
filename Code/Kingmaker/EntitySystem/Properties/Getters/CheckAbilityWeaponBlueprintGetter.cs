using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("83a8a95dacc24c6f8bb7fdccf7ec43ea")]
public class CheckAbilityWeaponBlueprintGetter : UnitPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[SerializeField]
	[InfoBox("Checks whether the ability weapon OR its prototypes match the specified blueprint")]
	private BlueprintItemWeaponReference m_Weapon;

	private BlueprintItemWeapon Weapon => m_Weapon?.Get();

	protected override int GetBaseValue()
	{
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		for (BlueprintItemWeapon blueprintItemWeapon = abilityWeapon?.Blueprint?.PrototypeLink as BlueprintItemWeapon; blueprintItemWeapon != null; blueprintItemWeapon = (BlueprintItemWeapon)blueprintItemWeapon.PrototypeLink)
		{
			if (blueprintItemWeapon == Weapon)
			{
				return 1;
			}
		}
		if (abilityWeapon?.Blueprint != Weapon)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability weapon OR prototype BP is " + (Weapon?.name ?? "null");
	}
}
