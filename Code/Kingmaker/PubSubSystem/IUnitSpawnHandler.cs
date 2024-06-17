using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitSpawnHandler<TTag> : IUnitSpawnHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IEventTag<IUnitSpawnHandler, TTag>
{
}
