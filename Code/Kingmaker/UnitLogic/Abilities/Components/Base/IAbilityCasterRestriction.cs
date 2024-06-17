using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityCasterRestriction
{
	bool IsCasterRestrictionPassed(MechanicEntity caster);

	string GetAbilityCasterRestrictionUIText(MechanicEntity caster);
}
