using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3584620744644ad6932525e79c8ca8e6")]
public class CheckAbilityWeaponIsSourceOfFactGetter : PropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalFact
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is Fact From Weapon";
	}

	protected override int GetBaseValue()
	{
		MechanicEntityFact fact = this.GetFact();
		ItemEntityWeapon abilityWeapon = this.GetAbilityWeapon();
		if (fact == null || abilityWeapon == null || !fact.IsFrom(abilityWeapon))
		{
			return 0;
		}
		return 1;
	}
}
