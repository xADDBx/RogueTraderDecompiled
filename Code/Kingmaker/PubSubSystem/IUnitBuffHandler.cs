using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs;

namespace Kingmaker.PubSubSystem;

public interface IUnitBuffHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleBuffDidAdded(Buff buff);

	void HandleBuffDidRemoved(Buff buff);

	void HandleBuffRankIncreased(Buff buff);

	void HandleBuffRankDecreased(Buff buff);
}
public interface IUnitBuffHandler<TTag> : IEventTag<IUnitBuffHandler, TTag>, IUnitBuffHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEntitySubscriber
{
}
