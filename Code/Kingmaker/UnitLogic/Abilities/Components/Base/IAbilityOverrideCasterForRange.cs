using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityOverrideCasterForRange
{
	MechanicEntity GetCaster(MechanicEntity originalCaster, bool forUi);
}
