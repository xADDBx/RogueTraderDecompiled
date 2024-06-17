using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityCannotUseInThreateningArea : ISubscriber
{
	void HandleCannotUseAbilityInThreateningArea(AbilityData ability);
}
