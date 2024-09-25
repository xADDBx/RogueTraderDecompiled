using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.PubSubSystem;

public interface IAbilityTargetSelectionUIHandler : ISubscriber
{
	void HandleAbilityTargetSelectionStart(AbilityData ability);

	void HandleAbilityTargetSelectionEnd(AbilityData ability);
}
