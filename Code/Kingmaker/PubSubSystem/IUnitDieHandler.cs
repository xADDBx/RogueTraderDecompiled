using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitDieHandler : ISubscriber<IAbstractUnitEntity>, ISubscriber
{
	void OnUnitDie();
}
public interface IUnitDieHandler<TTag> : IUnitDieHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitDieHandler, TTag>
{
}
