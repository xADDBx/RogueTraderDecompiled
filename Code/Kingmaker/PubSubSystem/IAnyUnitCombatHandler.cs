using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IAnyUnitCombatHandler : ISubscriber
{
	void HandleUnitJoinCombat(BaseUnitEntity unit);

	void HandleUnitLeaveCombat(BaseUnitEntity unit);
}
