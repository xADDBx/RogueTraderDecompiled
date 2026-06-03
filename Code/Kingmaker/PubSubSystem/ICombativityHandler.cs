using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ICombativityHandler : ISubscriber
{
	void HandleCombativityModifierAdded(float max, CombativityModifier modifier);

	void HandleCombativityModifierRemoved(float max, CombativityModifier modifier);
}
