using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Items;

public interface IEquipItemHandler : ISubscriber<IItemEntity>, ISubscriber
{
	void OnDidEquipped();

	void OnWillUnequip();
}
public interface IEquipItemHandler<TTag> : IEquipItemHandler, ISubscriber<IItemEntity>, ISubscriber, IEventTag<IEquipItemHandler, TTag>
{
}
