using Kingmaker.EntitySystem.Entities;
using Kingmaker.ResourceLinks;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityCustomAnimation
{
	UnitAnimationActionLink GetAbilityAction(BaseUnitEntity caster);
}
