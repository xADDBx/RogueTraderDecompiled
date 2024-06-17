using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitEquipmentHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem);
}
public interface IUnitEquipmentHandler<TTag> : IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, TTag>
{
}
