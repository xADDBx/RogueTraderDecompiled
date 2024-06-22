using System;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;

public class CargoDetailedZoneVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, ICargoSelectHandler, ISubscriber
{
	public readonly ReactiveCollection<CargoSlotVM> CargoSlots;

	public readonly ItemsFilterSearchVM ItemsFilterSearchVM;

	public readonly ReactiveCommand<CargoSlotVM> SlotToScroll = new ReactiveCommand<CargoSlotVM>();

	private ReactiveProperty<string> SearchString { get; } = new ReactiveProperty<string>();


	public CargoDetailedZoneVM(ReactiveCollection<CargoSlotVM> cargoSlots)
	{
		CargoSlots = cargoSlots;
		AddDisposable(ItemsFilterSearchVM = new ItemsFilterSearchVM(SearchString));
		AddDisposable(SearchString.Subscribe(SetSearch));
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
	}

	private void SetSearch(string text)
	{
		ItemsFilterSearchVM.SetSearchString(text);
		foreach (CargoSlotVM cargoSlot in CargoSlots)
		{
			cargoSlot.SearchString = text;
		}
	}

	private void HandleScrollToElement(CargoSlotVM slot)
	{
		SlotToScroll?.Execute(slot);
	}

	public void HandleSelectCargo(CargoSlotVM cargoSlot)
	{
		HandleScrollToElement(cargoSlot);
	}
}
