using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.View.Vendor;
using Kingmaker.Code.UI.MVVM.VM.ContextMenu;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Controllers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorReputationPartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVendorAddToSellCargoHandler, ISubscriber, IGainFactionReputationHandler
{
	public readonly InventoryCargoVM InventoryCargoVM;

	public readonly ReactiveProperty<int> VendorReputationLevel = new ReactiveProperty<int>();

	public readonly ReactiveProperty<int?> VendorReputationProgressToNextLevel = new ReactiveProperty<int?>();

	public readonly ReactiveProperty<float> VendorCurrentReputationProgress = new ReactiveProperty<float>();

	public readonly VendorReputationForItemWindowVM VendorReputationForItemWindow;

	public readonly LensSelectorVM Selector;

	public readonly ReactiveProperty<int?> ReputationPoints = new ReactiveProperty<int?>(0);

	public readonly ReactiveProperty<int?> NextLevelReputationPoints = new ReactiveProperty<int?>(0);

	public readonly ReactiveProperty<int> Delta = new ReactiveProperty<int>();

	public readonly ReactiveProperty<float> Difference = new ReactiveProperty<float>();

	public readonly ReactiveProperty<int> ExchangeValue = new ReactiveProperty<int>();

	public readonly List<ItemsItemOrigin> AcceptItems = new List<ItemsItemOrigin>();

	public readonly BoolReactiveProperty CanSellCargo = new BoolReactiveProperty();

	public readonly ReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	public readonly ReactiveProperty<bool> IsMaxLevel = new ReactiveProperty<bool>();

	public readonly bool NeedHidePfAndReputation;

	public readonly BoolReactiveProperty HasItemsToSell = new BoolReactiveProperty();

	public readonly string VendorFractionName;

	private VendorLogic Vendor => VendorHelper.Vendor;

	private BlueprintVendorFaction VendorFaction => Game.Instance.Vendor.VendorFaction;

	public VendorReputationPartVM(InventoryCargoVM inventoryCargoVM)
	{
		AddDisposable(EventBus.Subscribe(this));
		InventoryCargoVM = inventoryCargoVM;
		NeedHidePfAndReputation = Vendor.NeedHidePfAndReputation;
		VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(VendorFaction.FactionType);
		VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(VendorFaction.FactionType);
		VendorCurrentReputationProgress.Value = ReputationHelper.GetCurrentReputationPoints(VendorFaction.FactionType);
		ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.Value);
		NextLevelReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.Value + 1);
		Delta.Value = (NextLevelReputationPoints.Value - ReputationPoints.Value).Value;
		Difference.Value = (VendorCurrentReputationProgress.Value - (float?)ReputationPoints.Value).Value;
		VendorFractionName = VendorFaction.DisplayName.Text;
		AcceptItems = Vendor.VendorFaction.CargoTypes.ToList();
		AddDisposable(VendorReputationForItemWindow = new VendorReputationForItemWindowVM(AcceptItems));
		AddDisposable(Selector = new LensSelectorVM());
		IsMaxLevel.Value = ReputationHelper.IsMaxReputation(VendorFaction.FactionType);
		AddDisposable(InventoryCargoVM.CargoSlots.ObserveAdd().Subscribe(delegate
		{
			CheckItemsToSell();
		}));
		AddDisposable(InventoryCargoVM.CargoSlots.ObserveRemove().Subscribe(delegate
		{
			CheckItemsToSell();
		}));
		CheckItemsToSell();
	}

	protected override void DisposeImplementation()
	{
		UnselectAll();
	}

	public void SellCargo()
	{
		UISounds.Instance.Sounds.Vendor.SellCargo.Play();
		Game.Instance.GameCommandQueue.DealSellCargoes(Vendor.VendorEntity);
	}

	public void HandleSellChange()
	{
		CanSellCargo.Value = Vendor.CargoesToSell.Count > 0;
		ChangeRepValue();
		CheckItemsToSell();
	}

	public void ChangeRepValue()
	{
		int num = 0;
		foreach (EntityRef<CargoEntity> item in Vendor.CargoesToSell)
		{
			num += item.Entity?.ReputationPointsCost ?? 0;
		}
		ExchangeValue.Value = num;
	}

	public void HideUnrelevant()
	{
		UnselectAll();
		InventoryCargoVM.SwitchUnrelevantVisibility();
	}

	public void CheckItemsToSell()
	{
		HasItemsToSell.Value = InventoryCargoVM.CargoSlots.Any((CargoSlotVM x) => x.CanCheck);
	}

	public void SelectAll()
	{
		foreach (CargoSlotVM cargoSlot in InventoryCargoVM.CargoSlots)
		{
			if (cargoSlot.CanCheck)
			{
				cargoSlot.AddCargoToSell();
			}
		}
	}

	public void UnselectAll()
	{
		foreach (CargoSlotVM cargoSlot in InventoryCargoVM.CargoSlots)
		{
			if (cargoSlot.CanCheck)
			{
				cargoSlot.RemoveCargoFromSell();
			}
		}
	}

	public void HandleGainFactionReputation(FactionType factionType, int count)
	{
		int currentReputationPoints = ReputationHelper.GetCurrentReputationPoints(VendorFaction.FactionType);
		if (!IsMaxLevel.Value)
		{
			VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(VendorFaction.FactionType);
			VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(VendorFaction.FactionType);
			ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.Value);
			NextLevelReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(VendorFaction.FactionType, VendorReputationLevel.Value + 1);
			Delta.Value = (NextLevelReputationPoints.Value - ReputationPoints.Value).Value;
			Difference.Value = (currentReputationPoints - ReputationPoints.Value).Value;
		}
		VendorCurrentReputationProgress.Value = currentReputationPoints;
		IsMaxLevel.Value = ReputationHelper.IsMaxReputation(VendorFaction.FactionType);
	}
}
