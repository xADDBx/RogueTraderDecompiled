using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.View.Mechadendrites;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Serializable]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("05f57cde631729b47a2599acad016423")]
public class AbilityCasterMainWeaponHasAmmo : BlueprintComponent, IAbilityCasterRestriction
{
	public bool SecondWeapon;

	public bool IsCasterRestrictionPassed(MechanicEntity caster)
	{
		ItemEntityWeapon itemEntityWeapon = ((!SecondWeapon) ? caster.GetFirstWeapon() : caster.GetSecondWeapon());
		if (itemEntityWeapon == null || (SecondWeapon && caster.GetSecondWeapon().Blueprint.IsTwoHanded && caster.Parts.GetOptional<UnitPartMechadendrites>() == null))
		{
			return false;
		}
		if (itemEntityWeapon.CurrentAmmo <= 0)
		{
			return itemEntityWeapon.Blueprint.IsMelee;
		}
		return true;
	}

	public string GetAbilityCasterRestrictionUIText(MechanicEntity caster)
	{
		if (((!SecondWeapon) ? caster.GetFirstWeapon() : caster.GetSecondWeapon()) != null)
		{
			return LocalizedTexts.Instance.Reasons.NotEnoughAmmo;
		}
		return "";
	}
}
