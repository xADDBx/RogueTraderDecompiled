using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem.Core;

public interface IUnitHandler : IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void HandleUnitDestroyed();

	void HandleUnitDeath();
}
public interface IUnitHandler<TTag> : IUnitHandler, IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitHandler, TTag>
{
}
