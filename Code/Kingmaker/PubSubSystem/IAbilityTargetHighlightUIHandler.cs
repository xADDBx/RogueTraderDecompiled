using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityTargetHighlightUIHandler : ISubscriber
{
	void HandleAbilityShowSelection(AbilityData ability, bool state);
}
