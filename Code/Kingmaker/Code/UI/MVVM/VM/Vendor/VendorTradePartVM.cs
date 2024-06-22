using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Controllers;
using Kingmaker.Enums;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.UI.MVVM;
using UniRx;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.VM.Vendor;

public class VendorTradePartVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, IVendorDealHandler, ISubscriber, IVendorTransferHandler
{
	public readonly ProfitFactorVM ProfitFactorVM;

	public readonly ReactiveProperty<string> VendorName = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<FactionType> VendorFaction = new ReactiveProperty<FactionType>();

	public readonly ReactiveProperty<Sprite> VendorSprite = new ReactiveProperty<Sprite>();

	public readonly ReactiveProperty<int> VendorReputationLevel = new ReactiveProperty<int>();

	public Sprite PlayerPortrait;

	public string PlayerName;

	public string ProfitFactorText;

	public readonly ReactiveProperty<int?> VendorReputationProgressToNextLevel = new ReactiveProperty<int?>();

	public readonly ReactiveProperty<float> VendorCurrentReputationProgress = new ReactiveProperty<float>();

	public readonly ReactiveProperty<bool> IsPossibleDeal = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<float> DealPrice = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<int?> ReputationPoints = new ReactiveProperty<int?>(0);

	public readonly ReactiveProperty<int?> NextLevelReputationPoints = new ReactiveProperty<int?>(0);

	public ReactiveProperty<VendorTransitionWindowVM> TransitionWindowVM = new ReactiveProperty<VendorTransitionWindowVM>();

	public List<VendorLevelItemsVM> EnableSlots = new List<VendorLevelItemsVM>();

	public ReactiveCommand OnSlotsUpdate = new ReactiveCommand();

	public readonly string VendorFractionName;

	public readonly ReactiveProperty<float> TotalPFValue = new ReactiveProperty<float>(0f);

	public readonly ReactiveProperty<float> CurrentPFValue = new ReactiveProperty<float>(0f);

	public readonly BoolReactiveProperty ReadyToSellCargo = new BoolReactiveProperty();

	public readonly BoolReactiveProperty VendorHasItemsToSell = new BoolReactiveProperty();

	public readonly ReactiveProperty<int> Delta = new ReactiveProperty<int>();

	public readonly ReactiveProperty<float> Difference = new ReactiveProperty<float>();

	public readonly ReactiveProperty<bool> IsMaxLevel = new ReactiveProperty<bool>();

	public readonly bool NeedHidePfAndReputation;

	private VendorLogic Vendor => VendorHelper.Vendor;

	public bool HasDiscount
	{
		get
		{
			int value;
			if (Game.Instance.Vendor.VendorEntity != null)
			{
				return Game.Instance.Player.ProfitFactor.VendorDiscounts.TryGetValue(Vendor.VendorFaction.FactionType, out value);
			}
			return false;
		}
	}

	public int DiscountValue
	{
		get
		{
			if (Game.Instance.Vendor.VendorEntity == null)
			{
				return 0;
			}
			return Game.Instance.Player.ProfitFactor.VendorDiscounts.GetValueOrDefault(Vendor.VendorFaction.FactionType, 0);
		}
	}

	public VendorTradePartVM()
	{
		AddDisposable(EventBus.Subscribe(this));
		SetupSlots();
		AddDisposable(ProfitFactorVM = new ProfitFactorVM());
		AddDisposable(MainThreadDispatcher.FrequentUpdateAsObservable().Subscribe(delegate
		{
			OnUpdateHandler();
		}));
		VendorFractionName = Game.Instance.Vendor.VendorFaction.DisplayName.Text;
		NeedHidePfAndReputation = Game.Instance.Vendor.NeedHidePfAndReputation;
		VendorFaction.Value = Game.Instance.Vendor.VendorFaction.FactionType;
		ReactiveProperty<string> vendorName = VendorName;
		object obj = Game.Instance.Vendor.VendorName ?? string.Empty;
		if (obj == null)
		{
			obj = "";
		}
		vendorName.Value = (string)obj;
		VendorSprite.Value = Game.Instance.Vendor.VendorPortrait?.SmallPortrait;
		PlayerName = Game.Instance.Player.MainCharacterEntity.Name;
		PlayerPortrait = Game.Instance.Player.MainCharacterEntity?.Portrait?.SmallPortrait;
		ProfitFactorText = UIStrings.Instance.ProfitFactorTexts.Title;
		VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(Game.Instance.Vendor.VendorFaction.FactionType);
		VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(Game.Instance.Vendor.VendorFaction.FactionType);
		VendorCurrentReputationProgress.Value = ReputationHelper.GetCurrentReputationPoints(Game.Instance.Vendor.VendorFaction.FactionType);
		ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, VendorReputationLevel.Value);
		NextLevelReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, VendorReputationLevel.Value + 1);
		Delta.Value = (NextLevelReputationPoints.Value - ReputationPoints.Value).Value;
		Difference.Value = (VendorCurrentReputationProgress.Value - (float?)ReputationPoints.Value).Value;
		IsMaxLevel.Value = ReputationHelper.IsMaxReputation(VendorFaction.Value);
	}

	protected override void DisposeImplementation()
	{
		EnableSlots.ForEach(delegate(VendorLevelItemsVM s)
		{
			s.Dispose();
		});
		EnableSlots.Clear();
	}

	private void SetupSlots()
	{
		EnableSlots.ForEach(delegate(VendorLevelItemsVM s)
		{
			s.Dispose();
		});
		EnableSlots.Clear();
		int currentReputationLevel = ReputationHelper.GetCurrentReputationLevel(Game.Instance.Vendor.VendorFactionType);
		List<ItemEntity> list = Vendor.StoreItems.OrderBy((ItemEntity item) => Game.Instance.Vendor.VendorInventory.GetVendorLootItem(item).ReputationToUnlock).ToList();
		int num = -2;
		int num2 = 0;
		VendorLevelItemsVM vendorLevelItemsVM = null;
		ItemEntity itemEntity = list.LastOrDefault();
		foreach (ItemEntity item in list)
		{
			if (item != null)
			{
				int reputationToUnlock = Game.Instance.Vendor.VendorInventory.GetVendorLootItem(item).ReputationToUnlock;
				int reputationLevelByPoints = ReputationHelper.GetReputationLevelByPoints(Game.Instance.Vendor.VendorFactionType, reputationToUnlock);
				if (reputationLevelByPoints > num)
				{
					vendorLevelItemsVM = new VendorLevelItemsVM(reputationLevelByPoints, reputationLevelByPoints > currentReputationLevel, item == itemEntity);
					num = reputationLevelByPoints;
					EnableSlots.Add(vendorLevelItemsVM);
				}
				vendorLevelItemsVM?.AddItem(item, num2);
				num2++;
			}
		}
		VendorHasItemsToSell.Value = EnableSlots.Count > 0;
		OnSlotsUpdate?.Execute();
	}

	private void OnUpdateHandler()
	{
		IsPossibleDeal.Value = Vendor.IsDealPossible;
		DealPrice.Value = Vendor.DealPrice;
		TotalPFValue.Value = Game.Instance.Player.ProfitFactor.Total;
		CurrentPFValue.Value = Game.Instance.Player.ProfitFactor.Total;
		ReadyToSellCargo.Value = Vendor.CargoesToSell.Count > 0;
		if (!IsMaxLevel.Value)
		{
			VendorReputationLevel.Value = ReputationHelper.GetCurrentReputationLevel(Game.Instance.Vendor.VendorFaction.FactionType);
			VendorReputationProgressToNextLevel.Value = ReputationHelper.GetNextLevelReputationPoints(Game.Instance.Vendor.VendorFaction.FactionType);
			NextLevelReputationPoints.Value = ReputationHelper.GetNextLevelReputationPoints(Game.Instance.Vendor.VendorFaction.FactionType);
			VendorCurrentReputationProgress.Value = ReputationHelper.GetCurrentReputationPoints(Game.Instance.Vendor.VendorFaction.FactionType);
			ReputationPoints.Value = ReputationHelper.GetReputationPointsByLevel(Game.Instance.Vendor.VendorFaction.FactionType, VendorReputationLevel.Value);
			Delta.Value = (NextLevelReputationPoints.Value - ReputationPoints.Value).Value;
			Difference.Value = (VendorCurrentReputationProgress.Value - (float?)ReputationPoints.Value).Value;
		}
	}

	public void HandleTransitionWindow(ItemEntity itemEntity = null)
	{
		CloseTransitionWindow();
		TransitionWindowVM.Value = new VendorTransitionWindowVM(Vendor, itemEntity, CloseTransitionWindow);
	}

	private void CloseTransitionWindow()
	{
		TransitionWindowVM.Value?.Dispose();
		TransitionWindowVM.Value = null;
	}

	void IVendorDealHandler.HandleVendorDeal()
	{
		SetupSlots();
	}

	void IVendorDealHandler.HandleCancelVendorDeal()
	{
	}
}
