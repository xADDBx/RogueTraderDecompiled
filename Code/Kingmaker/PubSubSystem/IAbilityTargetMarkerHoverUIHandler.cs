using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityTargetMarkerHoverUIHandler : ISubscriber
{
	void HandleAbilityTargetMarkerHover(AbilityData ability, bool hover);
}
