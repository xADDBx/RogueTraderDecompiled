using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IUnitMovedByAbilityHandler : ISubscriber
{
	void HandleUnitMovedByAbility(AbilityExecutionContext abilityExecutionContext, int distanceInCells);
}
