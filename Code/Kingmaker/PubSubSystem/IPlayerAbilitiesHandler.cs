using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IPlayerAbilitiesHandler : ISubscriber
{
	void HandleAbilityAdded(Ability ability);

	void HandleAbilityRemoved(Ability ability);
}
