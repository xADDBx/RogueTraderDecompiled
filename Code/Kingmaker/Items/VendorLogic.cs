using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Root.Strings.GameLog;
using Kingmaker.Cargo;
using Kingmaker.Code.Globalmap.Colonization;
using Kingmaker.Code.UI.MVVM.VM.WarningNotification;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Items;

public class VendorLogic : IController, IChangeChapterHandler, ISubscriber
{
	public static readonly TimeSpan CleanupStoreItemsFrequency = 1.Weeks();

	private MechanicEntity m_VendorEntity;

	private ChangeVendorPrices m_VendorPrices;

	public float DealPrice { get; private set; }

	public float DealWeight { get; private set; }

	public float ItemsForBuyWeight
	{
		get
		{
			float num = 0f;
			foreach (ItemEntity item in ItemsForBuy)
			{
				num += item.Blueprint.Weight * (float)item.Count;
			}
			return num;
		}
	}

	private static Player Player => Game.Instance.Player;

	private static CargoState CargoState => Player.CargoState;

	public bool IsTrading => m_VendorEntity != null;

	public MechanicEntity VendorEntity => m_VendorEntity;

	public BlueprintVendorFaction VendorFaction => m_VendorEntity.Parts.GetRequired<PartVendor>().Faction;

	public FactionType VendorFactionType => VendorFaction.FactionType;

	private BaseUnitEntity VendorUnit => m_VendorEntity as BaseUnitEntity;

	public string VendorName => VendorUnit?.Blueprint?.CharacterName ?? VendorEntity.Blueprint.Name;

	public PortraitData VendorPortrait => VendorUnit?.Portrait;

	public Sprite VendorIcon => m_VendorEntity?.Blueprint?.Icon;

	public PartVendor VendorInventory => m_VendorEntity?.GetOptional<PartVendor>();

	public bool NeedHidePfAndReputation
	{
		get
		{
			if (m_VendorEntity != null)
			{
				return m_VendorEntity.Blueprint.GetComponent<AddSharedVendor>().NeedHidePfAndReputation;
			}
			return false;
		}
	}

	public ItemsCollection StoreItems => VendorInventory?.Collection;

	public ItemsCollection ItemsForBuy => Player.UISettings.ItemsForBuy;

	public bool IsDealPossible
	{
		get
		{
			if (ItemsForBuy.Items.Count > 0)
			{
				return Player.ProfitFactor.Total + DealPrice >= 0f;
			}
			return false;
		}
	}

	public bool IsChanged
	{
		get
		{
			if (IsTrading)
			{
				if (ItemsForBuy.Items.Count <= 0 && DealPrice == 0f)
				{
					return DealWeight != 0f;
				}
				return true;
			}
			return false;
		}
	}

	public IReadOnlyCollection<EntityRef<CargoEntity>> CargoesToSell => Player.UISettings.CargoesToSell;

	public void CancelSellCargoesDeal()
	{
		Player.UISettings.CargoesToSell.Clear();
	}

	public void DealSellCargoes(MechanicEntity vendor)
	{
		CargoState.SellCargoes(Player.UISettings.CargoesToSell.Select((EntityRef<CargoEntity> x) => x.Entity).ToList(), vendor.GetRequired<PartVendor>().Faction.FactionType);
		Player.UISettings.CargoesToSell.Clear();
		EventBus.RaiseEvent(delegate(IVendorAddToSellCargoHandler h)
		{
			h.HandleSellChange();
		});
	}

	public void AddToSell(CargoEntity cargo)
	{
		if (CanAddToSell(cargo))
		{
			Player.UISettings.CargoesToSell.Add(cargo);
			EventBus.RaiseEvent(delegate(IVendorAddToSellCargoHandler h)
			{
				h.HandleSellChange();
			});
		}
	}

	public void RemoveFromSell(CargoEntity cargo)
	{
		Player.UISettings.CargoesToSell.Remove(cargo);
		EventBus.RaiseEvent(delegate(IVendorAddToSellCargoHandler h)
		{
			h.HandleSellChange();
		});
	}

	public bool CanAddToSell(CargoEntity cargoEntity)
	{
		if (cargoEntity.CanSell && CanVendorBuyCargo(cargoEntity))
		{
			return Player.UISettings.CargoesToSell.All((EntityRef<CargoEntity> x) => x != cargoEntity);
		}
		return false;
	}

	public bool CanVendorBuyCargo(CargoEntity cargoEntity)
	{
		return VendorFaction.CargoTypes.Any((ItemsItemOrigin x) => x == cargoEntity.Blueprint.OriginType);
	}

	public bool CanRemoveFromSell(CargoEntity cargoEntity)
	{
		return Player.UISettings.CargoesToSell.Any((EntityRef<CargoEntity> x) => x == cargoEntity);
	}

	public void BeginTrading([NotNull] MechanicEntity vendor)
	{
		if (m_VendorEntity != null)
		{
			if (m_VendorEntity == vendor)
			{
				PFLog.Default.Warning($"Previous trading with the same vendor {m_VendorEntity} is not finished");
				return;
			}
			PFLog.Default.Error($"Previous trading with {m_VendorEntity} is not finished");
			EndTraiding();
		}
		m_VendorEntity = vendor;
		if (VendorInventory == null)
		{
			PFLog.Default.Error($"Trading with {vendor} can't start: no vendor table");
			return;
		}
		EventBus.RaiseEvent((IMechanicEntity)vendor, (Action<IVendorLogicStateChanged>)delegate(IVendorLogicStateChanged h)
		{
			h.HandleVendorAboutToTrading();
		}, isCheckRuntime: true);
		m_VendorPrices = vendor.Blueprint.GetComponent<ChangeVendorPrices>();
		foreach (ItemEntity item in VendorInventory)
		{
			item.Identify();
		}
		if (VendorInventory.AutoIdentifyPlayersInventory)
		{
			foreach (ItemEntity item2 in Game.Instance.Player.Inventory.Items)
			{
				item2.Identify();
			}
		}
		CleanupStoreItems();
		EventBus.RaiseEvent((IMechanicEntity)vendor, (Action<IVendorLogicStateChanged>)delegate(IVendorLogicStateChanged h)
		{
			h.HandleBeginTrading();
		}, isCheckRuntime: true);
		CacheDetectedVendor(vendor);
	}

	private static void CacheDetectedVendor([NotNull] MechanicEntity vendor)
	{
		if (!Player.VendorsData.DetectedVendors.Any((DetectedVendorData x) => x.EntityBlueprint == vendor.Blueprint))
		{
			BlueprintArea currentlyLoadedArea = Game.Instance.CurrentlyLoadedArea;
			if (currentlyLoadedArea == null)
			{
				PFLog.Default.Error("Can not detect vendor " + vendor.UniqueId + " cause no loaded area");
				return;
			}
			BlueprintAreaPart currentlyLoadedAreaPart = Game.Instance.CurrentlyLoadedAreaPart;
			int chapter = Game.Instance.Player.Chapter;
			PartLastDetectedLocation orCreate = vendor.GetOrCreate<PartLastDetectedLocation>();
			orCreate.DetectLocation(currentlyLoadedArea, currentlyLoadedAreaPart, chapter);
			Player.VendorsData.AddToDetected(vendor, currentlyLoadedArea, currentlyLoadedAreaPart, chapter);
			PFLog.Default.Log("Detect vendor {0}, {1}, {2}, {3}, {4}", vendor.UniqueId, vendor.GetOptional<PartVendor>()?.Faction, orCreate.Area, orCreate.AreaPart, chapter);
		}
	}

	private static void ClearDetectedVendorsCache()
	{
		Player.VendorsData.ClearDetected();
		PFLog.Default.Log("Clear detected vendors");
	}

	public void EndTraiding()
	{
		ReturnItems();
		UpdateDeal();
		m_VendorEntity = null;
		EventBus.RaiseEvent(delegate(IVendorLogicStateChanged h)
		{
			h.HandleEndTrading();
		});
	}

	private void CleanupStoreItems()
	{
		List<ItemEntity> list = new List<ItemEntity>();
		TimeSpan gameTime = Game.Instance.TimeController.GameTime;
		foreach (ItemEntity storeItem in StoreItems)
		{
			if (storeItem.SellTime.HasValue && !(gameTime - storeItem.SellTime.Value < CleanupStoreItemsFrequency) && CargoHelper.IsTrashItem(storeItem))
			{
				list.Add(storeItem);
			}
		}
		foreach (ItemEntity item in list)
		{
			StoreItems.Remove(item);
		}
	}

	private void ReturnItems()
	{
		if (m_VendorEntity != null)
		{
			ItemsForBuy.Items.ToArray().ForEach(delegate(ItemEntity i)
			{
				ItemsForBuy.Transfer(i, StoreItems);
			});
		}
		else
		{
			ItemsForBuy.RemoveAll();
		}
	}

	private void UpdateDeal()
	{
		DealPrice = 0f;
		DealWeight = 0f;
		foreach (ItemEntity item in ItemsForBuy)
		{
			DealPrice -= GetItemBuyPrice(item);
			DealWeight += item.Blueprint.Weight * (float)item.Count;
		}
		EventBus.RaiseEvent(delegate(IVendorDealPriceChangeHandler h)
		{
			h.HandleDealPriceChanged(DealPrice);
		});
	}

	public float GetItemBuyPrice(ItemEntity item)
	{
		return GetItemBuyPrice(item.Blueprint);
	}

	public float GetItemBuyPrice(BlueprintItem blueprintItem)
	{
		if (IsTrading)
		{
			if ((bool)m_VendorPrices)
			{
				return m_VendorPrices.GetProfitFactorCost(blueprintItem);
			}
			if ((bool)VendorInventory)
			{
				return VendorInventory.GetProfitFactorCost(blueprintItem);
			}
		}
		return blueprintItem.ProfitFactorCost;
	}

	public float GetItemBaseBuyPrice(BlueprintItem blueprintItem)
	{
		if (IsTrading)
		{
			if ((bool)m_VendorPrices)
			{
				return m_VendorPrices.GetProfitFactorCost(blueprintItem);
			}
			if ((bool)VendorInventory)
			{
				return VendorInventory.GetBaseProfitFactorCost(blueprintItem);
			}
		}
		return blueprintItem.ProfitFactorCost;
	}

	public ItemEntity AddForBuy(ItemEntity item, int count)
	{
		if (item.Collection != StoreItems)
		{
			PFLog.Default.Error("Item is not in 'store' collection");
			return null;
		}
		if (VendorInventory.IsLockedByReputation(item.Blueprint))
		{
			PFLog.Default.Log($"Item {item.Name} locked by reputation {VendorInventory.GetCurrentFactionReputationPoints()}/{VendorInventory.GetReputationToUnlock(item.Blueprint)}");
			return null;
		}
		count = ((count < 0) ? item.Count : count);
		ItemEntity result = item.Collection.Transfer(item, count, ItemsForBuy);
		UpdateDeal();
		return result;
	}

	public ItemEntity RemoveFromBuy(ItemEntity item, int count)
	{
		if (item.Collection != ItemsForBuy)
		{
			PFLog.Default.Error("Item is not in 'buy' collection");
			return null;
		}
		count = ((count < 0) ? item.Count : count);
		ItemEntity result = item.Collection.Transfer(item, count, StoreItems);
		UpdateDeal();
		return result;
	}

	public void CancelDeal()
	{
		List<ItemEntity> list = ItemsForBuy.Where((ItemEntity item) => item != null).ToList();
		if (list.Count > 0)
		{
			list.ForEach(delegate(ItemEntity item)
			{
				RemoveFromBuy(item, -1).TryMergeInCollection();
			});
			EventBus.RaiseEvent(delegate(IVendorDealHandler h)
			{
				h.HandleCancelVendorDeal();
			});
		}
	}

	public void MakeDealWithCurrentVendor()
	{
		Deal(m_VendorEntity);
	}

	public void Deal(MechanicEntity vendor)
	{
		if (!IsDealPossible)
		{
			PFLog.Default.Error("Trade deal is impossible");
			return;
		}
		Game.Instance.Statistic.HandleVendorDeal(vendor, ItemsForBuy);
		ItemEntity dealItem = null;
		ItemEntity[] array = ItemsForBuy.ToArray();
		foreach (ItemEntity itemEntity in array)
		{
			itemEntity.SellTime = null;
			itemEntity.SetVendorIfNull(VendorEntity);
			ItemsForBuy.Transfer(itemEntity, Player.Inventory);
			dealItem = itemEntity;
		}
		ReturnItems();
		UpdateDeal();
		EventBus.RaiseEvent(delegate(IVendorDealHandler h)
		{
			h.HandleVendorDeal();
		});
		EventBus.RaiseEvent(delegate(IVendorBuyHandler h)
		{
			h.HandleBuyItem(dealItem);
		});
		EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
		{
			h.HandleWarning(GameLogStrings.Instance.ItemGained.Message.Text + ". " + dealItem?.Name, addToLog: false, WarningNotificationFormat.Attention);
		});
	}

	void IChangeChapterHandler.HandleChangeChapter()
	{
		ClearDetectedVendorsCache();
	}

	public static void AddDiscount(FactionType faction, int discount)
	{
		if (faction != 0)
		{
			ProfitFactor profitFactor = Game.Instance.Player.ProfitFactor;
			if (profitFactor.VendorDiscounts.TryGetValue(faction, out var value))
			{
				profitFactor.VendorDiscounts[faction] = value + discount;
			}
			else
			{
				profitFactor.VendorDiscounts.Add(faction, discount);
			}
			EventBus.RaiseEvent(delegate(IGainFactionVendorDiscountHandler l)
			{
				l.HandleGainFactionVendorDiscount(faction, discount);
			});
		}
	}
}
