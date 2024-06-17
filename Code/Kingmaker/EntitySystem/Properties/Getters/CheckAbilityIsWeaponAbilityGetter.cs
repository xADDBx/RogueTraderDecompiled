using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Properties.BaseGetter;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("3d3ea03bc11a4a68ba661ee1b53388a8")]
public class CheckAbilityIsWeaponAbilityGetter : UnitPropertyGetter, PropertyContextAccessor.IOptionalAbilityWeapon, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	protected override int GetBaseValue()
	{
		if (this.GetAbilityWeapon() == null)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption()
	{
		return "Ability comes from a weapon or uses a weapon";
	}
}
