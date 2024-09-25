using Kingmaker.Code.UI.MVVM.View.Slots;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Sound;

namespace Kingmaker.Code.UI.MVVM.View.Loot;

public abstract class LootSlotView : ItemSlotView<ItemSlotVM>
{
	protected void OnClick()
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		EventBus.RaiseEvent(delegate(ILootHandler h)
		{
			h.HandleChangeLoot(base.ViewModel);
		});
	}

	protected void MoveToInventory(bool immediately)
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToInventory(base.ViewModel, immediately);
		});
	}

	protected void MoveToCargo(bool immediately)
	{
		UISounds.Instance.PlayItemSound(SlotAction.Put, base.ViewModel.ItemEntity, equipSound: false);
		EventBus.RaiseEvent(delegate(IInventoryHandler h)
		{
			h.TryMoveToCargo(base.ViewModel, immediately);
		});
	}

	protected void Split()
	{
		EventBus.RaiseEvent(delegate(INewSlotsHandler h)
		{
			h.HandleTrySplitSlot(base.ViewModel);
		});
	}
}
