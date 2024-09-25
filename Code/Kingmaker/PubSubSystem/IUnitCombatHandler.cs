using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitCombatHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitJoinCombat();

	void HandleUnitLeaveCombat();
}
public interface IUnitCombatHandler<TTag> : IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitCombatHandler, TTag>
{
}
