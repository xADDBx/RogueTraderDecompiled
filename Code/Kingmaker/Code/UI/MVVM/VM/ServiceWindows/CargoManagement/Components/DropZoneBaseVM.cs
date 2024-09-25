using System;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public abstract class DropZoneBaseVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IEquipSlotPossibleTarget, ISubscriber<ItemEntity>, ISubscriber
{
	public readonly ReactiveProperty<bool> HasDropItem = new ReactiveProperty<bool>();

	public readonly ReactiveProperty<bool> CanDropItem = new ReactiveProperty<bool>();

	private readonly Action<ItemSlotVM> m_DropVMAction;

	public DropZoneBaseVM(Action<ItemSlotVM> dropVMAction)
	{
		m_DropVMAction = dropVMAction;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	public void TryDropItem(ItemSlotVM slotVM)
	{
		ItemEntity itemEntity = slotVM.ItemEntity;
		if (CheckItem(itemEntity))
		{
			m_DropVMAction?.Invoke(slotVM);
		}
	}

	public void HandleHighlightStart(ItemEntity item)
	{
		HasDropItem.Value = item != null;
		CanDropItem.Value = CheckItem(item);
	}

	public void HandleHighlightStop()
	{
		HasDropItem.Value = false;
	}

	protected abstract bool CheckItem(ItemEntity itemEntity);
}
