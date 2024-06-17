using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface ILootDroppedAsAttachedHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleLootDroppedAsAttached();
}
public interface ILootDroppedAsAttachedHandler<TTag> : ILootDroppedAsAttachedHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<ILootDroppedAsAttachedHandler, TTag>
{
}
