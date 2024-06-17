using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityTargetHoverUIHandler : ISubscriber
{
	void HandleAbilityTargetHover(AbilityData ability, bool hover);
}
