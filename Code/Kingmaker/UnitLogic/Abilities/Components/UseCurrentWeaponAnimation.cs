using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("b4dae371427041c0a5a50b366d5a1b74")]
public class UseCurrentWeaponAnimation : BlueprintComponent
{
	public bool OnlyMeleeWeapon;

	public ItemEntityWeapon GetWeapon(MechanicEntity caster)
	{
		ItemEntityWeapon firstWeapon = caster.GetFirstWeapon();
		ItemEntityWeapon secondaryHandWeapon = caster.GetSecondaryHandWeapon();
		if (firstWeapon == null || (OnlyMeleeWeapon && !firstWeapon.Blueprint.IsMelee))
		{
			return secondaryHandWeapon;
		}
		return firstWeapon;
	}
}
