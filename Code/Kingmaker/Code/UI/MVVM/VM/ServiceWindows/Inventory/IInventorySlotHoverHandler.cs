using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public interface IInventorySlotHoverHandler : ISubscriber<ItemEntity>, ISubscriber
{
	void HandleHoverStart(ItemSlot slot, WeaponSlotType weaponSlotType);

	void HandleHoverStop();
}
