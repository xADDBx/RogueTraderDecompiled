using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityOwnerTargetSelectionHandler : ISubscriber
{
	void HandleOwnerAbilitySelected(AbilityData ability);
}
