using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.Common.Dropdown;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public class InventoryCargoVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICargoStateChangedHandler, ISubscriber, IVendorMassSellCargoHandler, ITransferItemHandler
{
	private const int MinSlots = 8;

	private const int MinVendorSlots = 14;

	public readonly InventoryCargoViewType CargoViewType;

	public readonly ReactiveProperty<CargoDropZoneVM> CargoDropZoneVM = new ReactiveProperty<CargoDropZoneVM>();

	public readonly ReactiveCommand<CargoSlotVM> SelectedCargo = new ReactiveCommand<CargoSlotVM>();

	public readonly ReactiveCollection<CargoSlotVM> CargoSlots = new ReactiveCollection<CargoSlotVM>();

	public readonly CargoDetailedZoneVM CargoZoneVM;

	public readonly bool FromPointOfInterest;

	private readonly Func<CargoEntity, bool> m_CargoFilter = (CargoEntity _) => true;

	public readonly ReactiveProperty<ItemsSorterType> CurrentSorter = new ReactiveProperty<ItemsSorterType>();

	private readonly Action m_CloseAction;

	public OwlcatDropdownVM SorterDropdownVM;

	public readonly BoolReactiveProperty HasVisibleCargo = new BoolReactiveProperty();

	public readonly BoolReactiveProperty HasAnyCargo = new BoolReactiveProperty();

	public readonly ReactiveCommand<CargoSlotVM> SlotToScroll = new ReactiveCommand<CargoSlotVM>();

	public readonly BoolReactiveProperty HideUnrelevant = new BoolReactiveProperty();

	public readonly BoolReactiveProperty IsCargoDetailedZone = new BoolReactiveProperty();

	public readonly bool IsCargoLocked;

	public InventoryCargoVM(InventoryCargoViewType viewType, Func<CargoEntity, bool> cargoFilter = null, Action closeCallback = null, bool fromPointOfInterest = false, bool fromVendor = false)
	{
		CargoViewType = viewType;
		if (cargoFilter != null)
		{
			m_CargoFilter = cargoFilter;
		}
		m_CloseAction = closeCallback;
		AddDisposable(EventBus.Subscribe(this));
		SetSorterDropDownVM(fromVendor);
		CollectSlots();
		CurrentSorter.Value = (CargoSlots.FirstOrDefault()?.ItemSlotsGroup?.SorterType?.Value).GetValueOrDefault();
		AddDisposable(CargoZoneVM = new CargoDetailedZoneVM(CargoSlots));
		TryCreateDropZone();
		FromPointOfInterest = fromPointOfInterest;
		AddDisposable(HideUnrelevant.Skip(1).Subscribe(delegate
		{
			CollectSlots();
		}));
		AddDisposable(CurrentSorter.Subscribe(OnSorterChanged));
		IsCargoLocked = Game.Instance.Player.CargoState.LockTransferFromCargo;
	}

	protected override void DisposeImplementation()
	{
		Clear();
		ClearDropZone();
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
	}

	private void SelectCargo(CargoSlotVM cargoSlotVM)
	{
		SelectedCargo?.Execute(cargoSlotVM);
		EventBus.RaiseEvent(delegate(ICargoSelectHandler l)
		{
			l.HandleSelectCargo(cargoSlotVM);
		});
	}

	public void TryCreateDropZone()
	{
		if (CargoDropZoneVM.Value == null)
		{
			CargoDropZoneVM disposable = (CargoDropZoneVM.Value = new CargoDropZoneVM(DropItem));
			AddDisposable(disposable);
		}
	}

	private void OnSorterChanged(ItemsSorterType sorter)
	{
		CargoSlots.ForEach(delegate(CargoSlotVM slotsGroupVm)
		{
			if (slotsGroupVm.ItemSlotsGroup != null)
			{
				slotsGroupVm.ItemSlotsGroup.SorterType.Value = sorter;
			}
		});
		List<CargoSlotVM> list = CargoSlots.ToTempList();
		list.Sort((CargoSlotVM a, CargoSlotVM b) => CargoSorter.CompareBy(a.CargoEntity, b.CargoEntity, sorter));
		CargoSlots.Clear();
		foreach (CargoSlotVM item in list)
		{
			CargoSlots.Add(item);
		}
	}

	public void SetCurrentSorter(ItemsSorterType newSorter)
	{
		CurrentSorter.Value = newSorter;
	}

	private void ClearDropZone()
	{
		CargoDropZoneVM.Value?.Dispose();
		CargoDropZoneVM.Value = null;
	}

	private void CollectSlots()
	{
		Clear();
		foreach (CargoEntity item2 in Game.Instance.Player.CargoState.CargoEntities.Where(m_CargoFilter))
		{
			CargoSlotVM cargoSlotVM = new CargoSlotVM(SelectCargo, item2, CargoViewType);
			if (CargoViewType == InventoryCargoViewType.Vendor && HideUnrelevant.Value && !IsCargoDetailedZone.Value)
			{
				if (cargoSlotVM.CanCheck)
				{
					CargoSlots.Add(cargoSlotVM);
				}
			}
			else
			{
				CargoSlots.Add(cargoSlotVM);
			}
		}
		int num = ((!RootUIContext.Instance.IsVendorShow) ? (8 - CargoSlots.Count) : (14 - CargoSlots.Count));
		if (num > 0)
		{
			for (int i = 0; i < num; i++)
			{
				CargoSlotVM item = new CargoSlotVM(null, null);
				CargoSlots.Add(item);
			}
		}
		HasVisibleCargo.Value = Enumerable.Any(CargoSlots, (CargoSlotVM slot) => !slot.IsEmpty);
		HasAnyCargo.Value = Game.Instance.Player.CargoState.CargoEntities.Any(m_CargoFilter);
		OnSorterChanged(CurrentSorter.Value);
	}

	public void SwitchUnrelevantVisibility()
	{
		HideUnrelevant.Value = !HideUnrelevant.Value;
	}

	private void Clear()
	{
		CargoSlots.ForEach(delegate(CargoSlotVM s)
		{
			s?.Dispose();
		});
		CargoSlots.Clear();
	}

	public void DropItem(ItemSlotVM item)
	{
		if (!(item.Group?.MechanicCollection.Owner is CargoEntity fromCargo) || fromCargo.CanTransferFromCargo(item.ItemEntity))
		{
			Game.Instance.GameCommandQueue.TransferItemsToCargo(new List<EntityRef<ItemEntity>> { item.ItemEntity });
		}
	}

	void ICargoStateChangedHandler.HandleCreateNewCargo(CargoEntity entity)
	{
		CollectSlots();
		CargoSlotVM slotVM = GetSlotVM(entity);
		SlotToScroll?.Execute(slotVM);
		CargoZoneVM.SlotToScroll?.Execute(slotVM);
	}

	void ICargoStateChangedHandler.HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
		if (!fromMassSell)
		{
			CollectSlots();
		}
	}

	public void HandleMassSellChange()
	{
		CollectSlots();
	}

	private void SetSorterDropDownVM(bool fromVendor)
	{
		List<DropdownItemVM> list = new List<DropdownItemVM>();
		foreach (ItemsSorterType value in Enum.GetValues(typeof(ItemsSorterType)))
		{
			if (fromVendor || value != ItemsSorterType.CargoValue)
			{
				list.Add(new DropdownItemVM(LocalizedTexts.Instance.ItemsFilter.GetText(value)));
			}
		}
		AddDisposable(SorterDropdownVM = new OwlcatDropdownVM(list));
	}

	void ICargoStateChangedHandler.HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
		CargoSlotVM slotVM = GetSlotVM(to);
		slotVM?.OnUpdateValues();
		HasVisibleCargo.Value = Enumerable.Any(CargoSlots, (CargoSlotVM slot) => !slot.IsEmpty);
		SlotToScroll?.Execute(slotVM);
		CargoZoneVM.SlotToScroll?.Execute(slotVM);
	}

	void ICargoStateChangedHandler.HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
	{
		GetSlotVM(from)?.OnUpdateValues();
		HasVisibleCargo.Value = Enumerable.Any(CargoSlots, (CargoSlotVM slot) => !slot.IsEmpty);
	}

	private CargoSlotVM GetSlotVM(CargoEntity entity)
	{
		return Enumerable.FirstOrDefault(CargoSlots, (CargoSlotVM s) => s.CargoEntity == entity);
	}

	void ITransferItemHandler.HandleTransferItem(ItemsCollection from, ItemsCollection to)
	{
		HasVisibleCargo.Value = Enumerable.Any(CargoSlots, (CargoSlotVM slot) => !slot.IsEmpty);
		HasAnyCargo.Value = Game.Instance.Player.CargoState.CargoEntities.Any(m_CargoFilter);
	}
}
