using System;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class InteractionSlotPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	public readonly string Name;

	public readonly string Description;

	private readonly Func<ItemEntity, bool> m_CanInsertItem;

	public readonly SlotsGroupVM<ItemSlotVM> Group;

	public readonly ReactiveProperty<OneSlotLootDropZoneVM> DropZoneVM = new ReactiveProperty<OneSlotLootDropZoneVM>();

	public readonly ReactiveProperty<ItemSlotVM> ItemSlot = new ReactiveProperty<ItemSlotVM>();

	public InteractionSlotPartVM(LootObjectVM lootObject, Func<ItemEntity, bool> canInsertItem)
	{
		Name = lootObject.DisplayName;
		Description = lootObject.Description;
		m_CanInsertItem = canInsertItem;
		Group = lootObject.SlotsGroup;
		UpdateItem();
		AddDisposable(Group.CollectionChangedCommand.Subscribe(delegate
		{
			UpdateItem();
		}));
		TryCreateDropZone();
	}

	private void TryCreateDropZone()
	{
		if (DropZoneVM.Value == null)
		{
			OneSlotLootDropZoneVM disposable = (DropZoneVM.Value = new OneSlotLootDropZoneVM(HandleDropItem, m_CanInsertItem));
			AddDisposable(disposable);
		}
	}

	private void UpdateItem()
	{
		ItemSlot.Value = Group.VisibleCollection.FirstItem((ItemSlotVM slot) => slot?.ItemEntity != null && m_CanInsertItem(slot.ItemEntity));
	}

	protected override void DisposeImplementation()
	{
		ClearDropZone();
	}

	private void ClearDropZone()
	{
		DropZoneVM.Value?.Dispose();
		DropZoneVM.Value = null;
	}

	private void HandleDropItem(ItemSlotVM item)
	{
		if (item == null)
		{
			return;
		}
		InsertableLootSlotVM insertableLootSlotVM = item as InsertableLootSlotVM;
		if (insertableLootSlotVM != null && insertableLootSlotVM.CanInsert.Value)
		{
			EventBus.RaiseEvent(delegate(INewSlotsHandler h)
			{
				h.HandleTryInsertSlot(insertableLootSlotVM);
			});
		}
	}
}
