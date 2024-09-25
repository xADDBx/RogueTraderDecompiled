using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("ad05cd9aa5084d5eb9bb95bfdcb04c18")]
public class WeaponReloadLogic : BlueprintComponent
{
	public bool IsAvailable(AbilityData ability)
	{
		MechanicEntity caster = ability.Caster;
		ItemEntityWeapon weapon = ability.Weapon;
		if (caster == null || weapon == null)
		{
			return false;
		}
		if (!caster.IsPlayerFaction)
		{
			return !ability.HasEnoughAmmo;
		}
		return weapon.CurrentAmmo < weapon.Blueprint.WarhammerMaxAmmo;
	}
}
