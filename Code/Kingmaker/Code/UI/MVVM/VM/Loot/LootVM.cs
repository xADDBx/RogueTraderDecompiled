using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Cargo;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.CargoManagement.Components;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Code.UI.MVVM.VM.Tooltip.Utils;
using Kingmaker.Controllers.Dialog;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UI.Models;
using Kingmaker.UI.Sound;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.UI.MVVM;
using Owlcat.Runtime.UniRx;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.Loot;

public class LootVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable, INewSlotsHandler, ISubscriber, ICollectLootHandler, IMoveItemHandler, IFullScreenUIHandler, IInventoryHandler, ILootHandler, ITransferItemHandler, ICargoStateChangedHandler, ISplitItemHandler, ICargoSelectHandler
{
	public readonly LootCollectorVM LootCollector;

	public readonly InventoryStashVM InventoryStash;

	public readonly InteractionSlotPartVM InteractionSlot;

	public readonly PlayerStashVM PlayerStash;

	public readonly InventoryCargoVM CargoInventory;

	public readonly ReactiveProperty<ExitLocationWindowVM> ExitLocationWindowVM = new ReactiveProperty<ExitLocationWindowVM>();

	private readonly Action m_AreaTransitionCallback;

	private Action m_CloseCallback;

	private readonly List<ItemsCollection> m_ItemsCollections;

	private bool m_ContextLootIsDirty;

	public readonly ReactiveCommand LootUpdated = new ReactiveCommand();

	public readonly ReactiveCommand OpenDetailed = new ReactiveCommand();

	public readonly BoolReactiveProperty NoLoot = new BoolReactiveProperty();

	private bool m_AllCollected;

	private readonly Dictionary<LootObjectType, List<ItemEntity>> m_LootItemsByObjectType;

	public static readonly string CargoButtonInMenuHighlighterKey = "CargoButtonInMenuHighlighterKey";

	public readonly BoolReactiveProperty ExtendedView = new BoolReactiveProperty();

	public readonly LensSelectorVM Selector;

	public LootContextVM.LootWindowMode Mode { get; }

	public ReactiveCollection<LootObjectVM> ContextLoot { get; } = new ReactiveCollection<LootObjectVM>();


	public SkillCheckResult SkillCheckResult { get; }

	private IEnumerable<ItemEntity> AllItems => m_ItemsCollections.SelectMany((ItemsCollection collection) => collection.Items);

	private IEnumerable<ItemEntity> LootableItems => AllItems.Where((ItemEntity item) => IsLootable(item));

	public bool IsOneSlot => InteractionSlot != null;

	public bool IsPlayerStash => PlayerStash != null;

	private static bool IsLootable(ItemEntity item)
	{
		if (!item.IsAvailable())
		{
			return false;
		}
		if (!item.IsLootable)
		{
			return false;
		}
		ItemSlot holdingSlot = item.HoldingSlot;
		if (holdingSlot != null && !holdingSlot.CanRemoveItem())
		{
			return false;
		}
		return true;
	}

	public bool ToInventory(ItemEntity item)
	{
		return m_LootItemsByObjectType[LootObjectType.Normal].Contains(item);
	}

	public bool ToCargo(ItemEntity item)
	{
		return m_LootItemsByObjectType[LootObjectType.Trash].Contains(item);
	}

	private LootVM(LootContextVM.LootWindowMode mode, IEnumerable<ItemsCollection> lootCollections, Action closeCallback, Action leaveZoneAction = null, Func<ItemEntity, bool> canInsertItem = null, SkillCheckResult skillCheckResult = null)
	{
		m_AllCollected = false;
		Mode = mode;
		SkillCheckResult = skillCheckResult;
		m_CloseCallback = closeCallback;
		m_AreaTransitionCallback = leaveZoneAction;
		ExtendedView.Value = Game.Instance.Player.UISettings.LootExtendedView;
		m_ItemsCollections = new List<ItemsCollection>(lootCollections);
		m_LootItemsByObjectType = new Dictionary<LootObjectType, List<ItemEntity>>
		{
			{
				LootObjectType.Normal,
				new List<ItemEntity>()
			},
			{
				LootObjectType.Trash,
				new List<ItemEntity>()
			}
		};
		foreach (ItemEntity lootableItem in LootableItems)
		{
			m_LootItemsByObjectType[GetLootObjectType(lootableItem)].Add(lootableItem);
		}
		if (Mode != LootContextVM.LootWindowMode.OneSlot)
		{
			foreach (var (lootObjectType2, items) in m_LootItemsByObjectType)
			{
				var (displayName, description) = GetLootObjectStrings(lootObjectType2);
				AddLootObject(new LootObjectVM(lootObjectType2, displayName, description, null, m_ItemsCollections.First(), items, Mode));
				void AddLootObject(LootObjectVM lootObject)
				{
					ContextLoot.Add(lootObject);
					AddDisposable(lootObject);
					AddDisposable(UniRxExtensionMethods.Subscribe(lootObject.SlotsGroup.CollectionChangedCommand, delegate
					{
						LootUpdated.Execute();
					}));
				}
			}
		}
		else
		{
			var (displayName2, description2) = GetLootObjectStrings(LootObjectType.SingleSlot);
			AddLootObject(new LootObjectVM(LootObjectType.Normal, displayName2, description2, null, m_ItemsCollections.First(), null, Mode));
		}
		NoLoot.Value = m_LootItemsByObjectType[LootObjectType.Normal].Empty() && m_LootItemsByObjectType[LootObjectType.Trash].Empty();
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Loot);
		});
		AddDisposable(EventBus.Subscribe(this));
		switch (mode)
		{
		case LootContextVM.LootWindowMode.OneSlot:
			AddDisposable(InteractionSlot = new InteractionSlotPartVM(ContextLoot.FirstItem(), canInsertItem));
			break;
		case LootContextVM.LootWindowMode.PlayerChest:
			AddDisposable(PlayerStash = new PlayerStashVM(this));
			AddDisposable(CargoInventory = new InventoryCargoVM(InventoryCargoViewType.Loot));
			break;
		default:
			AddDisposable(LootCollector = new LootCollectorVM(this));
			AddDisposable(CargoInventory = new InventoryCargoVM(InventoryCargoViewType.Loot));
			break;
		}
		AddDisposable(InventoryStash = new InventoryStashVM(inventory: false, canInsertItem));
		AddDisposable(MainThreadDispatcher.LateUpdateAsObservable().Subscribe(Update));
		UIEventType eventType = mode switch
		{
			LootContextVM.LootWindowMode.Short => UIEventType.LootShortOpen, 
			LootContextVM.LootWindowMode.ShortUnit => UIEventType.LootShortUnitOpen, 
			LootContextVM.LootWindowMode.ZoneExit => UIEventType.LootZoneExitOpen, 
			LootContextVM.LootWindowMode.PlayerChest => UIEventType.LootPlayerChestOpen, 
			LootContextVM.LootWindowMode.StandardChest => UIEventType.LootStandardChestOpen, 
			LootContextVM.LootWindowMode.OneSlot => UIEventType.LootOneSlotOpen, 
			_ => throw new SwitchExpressionException(mode), 
		};
		AddDisposable(Selector = new LensSelectorVM());
		EventBus.RaiseEvent(delegate(IUIEventHandler h)
		{
			h.HandleUIEvent(eventType);
		});
	}

	public LootVM(LootContextVM.LootWindowMode mode, EntityViewBase[] objects, Action closeCallback)
		: this(mode, GetLootCollections(objects), closeCallback, null, GetCanInsertItemPred(mode, objects))
	{
	}

	public LootVM(LootContextVM.LootWindowMode mode, ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult)
		: this(mode, GetLootCollections(objects), closeCallback, null, null, skillCheckResult)
	{
		if (NoLoot.Value)
		{
			LeaveZone();
		}
	}

	public LootVM(LootContextVM.LootWindowMode mode, IEnumerable<LootWrapper> zoneLoot, Action leaveZoneAction, Action closeCallback)
		: this(mode, GetLootCollections(zoneLoot), closeCallback, leaveZoneAction)
	{
		if (NoLoot.Value)
		{
			LeaveZone();
		}
	}

	protected override void DisposeImplementation()
	{
		Game.Instance.Player.UISettings.LootExtendedView = ExtendedView.Value;
		EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
		{
			h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Loot);
		});
	}

	private static Func<ItemEntity, bool> GetCanInsertItemPred(LootContextVM.LootWindowMode mode, IEnumerable<EntityViewBase> objects)
	{
		foreach (EntityViewBase @object in objects)
		{
			if (!(@object.GetComponent<InteractionLoot>() == null))
			{
				InteractionLootPart optional = @object.Data.ToEntity().GetOptional<InteractionLootPart>();
				if (optional != null && mode == LootContextVM.LootWindowMode.OneSlot)
				{
					return optional.CanInsertItem;
				}
			}
		}
		return null;
	}

	public void ChangeView()
	{
		ExtendedView.Value = !ExtendedView.Value;
	}

	private static IEnumerable<ItemsCollection> GetLootCollections(IEnumerable<EntityViewBase> objects)
	{
		foreach (EntityViewBase @object in objects)
		{
			if (@object is UnitEntityView unitEntityView && unitEntityView.EntityData.Inventory.Collection != Game.Instance.Player.Inventory)
			{
				yield return unitEntityView.EntityData.Inventory.Collection;
			}
			else if (!(@object.GetComponent<InteractionLoot>() == null))
			{
				InteractionLootPart optional = @object.Data.ToEntity().GetOptional<InteractionLootPart>();
				if (optional != null)
				{
					yield return optional.Loot;
				}
			}
		}
	}

	private static IEnumerable<ItemsCollection> GetLootCollections(ILootable[] objects)
	{
		for (int i = 0; i < objects.Length; i++)
		{
			ItemsCollection items = objects[i].Items;
			if (items != null && !items.All((ItemEntity item) => !IsLootable(item)))
			{
				yield return items;
			}
		}
	}

	private static IEnumerable<ItemsCollection> GetLootCollections(IEnumerable<LootWrapper> zoneLoot)
	{
		foreach (LootWrapper item in zoneLoot)
		{
			ItemsCollection itemsCollection = item.Unit?.Inventory.Collection ?? item.InteractionLoot?.Loot;
			if (itemsCollection != null && !itemsCollection.All((ItemEntity item) => !IsLootable(item)))
			{
				yield return itemsCollection;
			}
		}
	}

	private void MarkContextLootAsDirty()
	{
		m_ContextLootIsDirty = true;
	}

	private void Update()
	{
		if (m_ContextLootIsDirty)
		{
			m_ContextLootIsDirty = false;
			UpdateContextLoot();
		}
	}

	private void UpdateContextLoot()
	{
		foreach (ItemEntity lootableItem in LootableItems)
		{
			if (!ToInventory(lootableItem) && !ToCargo(lootableItem))
			{
				m_LootItemsByObjectType[GetLootObjectType(lootableItem)].Add(lootableItem);
			}
		}
		m_LootItemsByObjectType[LootObjectType.Normal].RemoveAll((ItemEntity i) => !LootableItems.Contains(i));
		m_LootItemsByObjectType[LootObjectType.Trash].RemoveAll((ItemEntity i) => !LootableItems.Contains(i));
		foreach (LootObjectVM item in ContextLoot)
		{
			item.SetNewItems(m_LootItemsByObjectType[item.Type]);
		}
		NoLoot.Value = m_AllCollected || (m_LootItemsByObjectType[LootObjectType.Normal].Empty() && m_LootItemsByObjectType[LootObjectType.Trash].Empty());
	}

	private LootObjectType GetLootObjectType(ItemEntity item)
	{
		if (Mode == LootContextVM.LootWindowMode.PlayerChest)
		{
			return LootObjectType.Normal;
		}
		if (!CargoHelper.IsTrashItem(item) && !item.ToCargoAutomatically)
		{
			return LootObjectType.Normal;
		}
		return LootObjectType.Trash;
	}

	void ILootHandler.HandleChangeLoot(ItemSlotVM slot)
	{
		if (IsPlayerStash || IsOneSlot)
		{
			InventoryHelper.TryCollectLootSlot(slot);
			return;
		}
		if (ToInventory(slot.ItemEntity))
		{
			if (CargoHelper.CanTransferToCargo(slot.ItemEntity))
			{
				AddToTrashPart(slot.ItemEntity);
			}
		}
		else if (ToCargo(slot.ItemEntity))
		{
			if (CargoHelper.IsItemInCargo(slot.ItemEntity))
			{
				if (CargoHelper.CanTransferFromCargo(slot.ItemEntity))
				{
					AddToLootPart(slot.ItemEntity);
				}
			}
			else if (slot.ItemEntity != null && !CargoHelper.IsTrashItem(slot.ItemEntity))
			{
				AddToLootPart(slot.ItemEntity);
			}
		}
		MarkContextLootAsDirty();
	}

	private void AddToLootPart(ItemEntity item)
	{
		m_LootItemsByObjectType[LootObjectType.Normal].Add(item);
		m_LootItemsByObjectType[LootObjectType.Trash].Remove(item);
	}

	private void AddToTrashPart(ItemEntity item)
	{
		m_LootItemsByObjectType[LootObjectType.Normal].Remove(item);
		m_LootItemsByObjectType[LootObjectType.Trash].Add(item);
	}

	private (string, string) GetLootObjectStrings(LootObjectType lootObjectType)
	{
		if (Mode == LootContextVM.LootWindowMode.OneSlot)
		{
			InteractionLootPart interactionLootPart = m_ItemsCollections.First().ConcreteOwner?.GetOptional<InteractionLootPart>();
			if (interactionLootPart != null)
			{
				string item = (string.IsNullOrEmpty(interactionLootPart.GetName()) ? UIStrings.Instance.LootWindow.GetLootName(interactionLootPart.Settings.LootContainerType) : interactionLootPart.GetName());
				string description = interactionLootPart.GetDescription();
				return (item, description);
			}
		}
		return UIStrings.Instance.LootWindow.GetLootObjectStrings(lootObjectType);
	}

	public void TryCollectLoot()
	{
		foreach (LootObjectVM item in ContextLoot)
		{
			switch (item.Type)
			{
			case LootObjectType.Normal:
				if (item.HasLootableItems)
				{
					Game.Instance.GameCommandQueue.CollectLoot(item.LootableItems.Select((ItemEntity x) => new EntityRef<ItemEntity>(x)).ToList());
					m_AllCollected = true;
					MarkContextLootAsDirty();
				}
				break;
			case LootObjectType.Trash:
				if (item.HasItemsToCargo)
				{
					Game.Instance.GameCommandQueue.TransferItemsToCargo(item.ItemsToCargo.Select((ItemEntity x) => new EntityRef<ItemEntity>(x)).ToList());
					EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
					{
						h.HandleWarning(UIStrings.Instance.LootWindow.CargoCollectedFromLoot, addToLog: false);
					});
					EventBus.RaiseEvent(delegate(IUIHighlighter h)
					{
						h.HighlightOnce(CargoButtonInMenuHighlighterKey);
					});
				}
				break;
			}
		}
		m_LootItemsByObjectType[LootObjectType.Normal].Clear();
		m_LootItemsByObjectType[LootObjectType.Trash].Clear();
		if (!ExtendedView.Value)
		{
			LeaveZone();
		}
	}

	public void AddAllToCargoPart()
	{
		for (int i = 0; i < m_LootItemsByObjectType[LootObjectType.Normal].Count; i++)
		{
			ItemEntity item = m_LootItemsByObjectType[LootObjectType.Normal].ElementAt(i);
			if (CargoHelper.CanTransferToCargo(item))
			{
				AddToTrashPart(item);
				UISounds.Instance.PlayItemSound(SlotAction.Put, item, equipSound: false);
				i--;
			}
		}
		MarkContextLootAsDirty();
	}

	public void AddAllToInventoryPart()
	{
		for (int i = 0; i < m_LootItemsByObjectType[LootObjectType.Trash].Count; i++)
		{
			ItemEntity itemEntity = m_LootItemsByObjectType[LootObjectType.Trash].ElementAt(i);
			if (itemEntity != null && !CargoHelper.IsTrashItem(itemEntity))
			{
				AddToLootPart(itemEntity);
				UISounds.Instance.PlayItemSound(SlotAction.Put, itemEntity, equipSound: false);
				i--;
			}
		}
		MarkContextLootAsDirty();
	}

	public void HandleOpenExitWindow(Action tryCollect)
	{
		ExitLocationWindowVM disposable = (ExitLocationWindowVM.Value = new ExitLocationWindowVM(tryCollect, CloseExitWindow));
		AddDisposable(disposable);
	}

	public void CloseExitWindow()
	{
		ExitLocationWindowVM.Value?.Dispose();
		ExitLocationWindowVM.Value = null;
	}

	public void LeaveZone()
	{
		TooltipHelper.HideTooltip();
		Close();
		BaseLeaveZone();
	}

	public void BaseLeaveZone()
	{
		m_AreaTransitionCallback?.Invoke();
	}

	public void Close()
	{
		m_CloseCallback?.Invoke();
		m_CloseCallback = null;
	}

	void IInventoryHandler.Refresh()
	{
	}

	void IInventoryHandler.TryEquip(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryDrop(ItemSlotVM slot)
	{
	}

	void IInventoryHandler.TryMoveToCargo(ItemSlotVM slot, bool immediately)
	{
		if (slot?.ItemEntity == null || !slot.CanTransferToCargo)
		{
			return;
		}
		if (immediately)
		{
			CargoInventory.DropItem(slot);
			return;
		}
		if (!IsPlayerStash && !IsOneSlot)
		{
			m_LootItemsByObjectType[LootObjectType.Trash].Add(slot.ItemEntity);
		}
		InventoryHelper.TryTransferInventorySlot(slot, ContextLoot[1]);
	}

	void IInventoryHandler.TryMoveToInventory(ItemSlotVM slot, bool immediately)
	{
		if (slot?.ItemEntity != null && slot.CanTransferToInventory)
		{
			if (immediately)
			{
				InventoryHelper.TryMoveSlot(slot, InventoryStash.FirstEmptySlot, InteractionSlot);
				return;
			}
			m_LootItemsByObjectType[LootObjectType.Normal].Add(slot.ItemEntity);
			InventoryHelper.TryTransferInventorySlot(slot, ContextLoot[0]);
		}
	}

	void INewSlotsHandler.HandleTryInsertSlot(InsertableLootSlotVM slot)
	{
		InventoryHelper.InsertToInteractionSlot(slot, InteractionSlot);
	}

	void INewSlotsHandler.HandleTrySplitSlot(ItemSlotVM slot)
	{
		InventoryHelper.TrySplitSlot(slot, isLoot: true);
	}

	void INewSlotsHandler.HandleTryMoveSlot(ItemSlotVM from, ItemSlotVM to)
	{
		if (from?.ItemEntity == null)
		{
			return;
		}
		if (from.SlotsGroupType == ItemSlotsGroupType.Loot && to.SlotsGroupType == ItemSlotsGroupType.Loot)
		{
			if (from.Group != to.Group)
			{
				((ILootHandler)this).HandleChangeLoot(from);
			}
			return;
		}
		if (ContextLoot[1].ContainsSlot(to))
		{
			m_LootItemsByObjectType[LootObjectType.Trash].Add(from.ItemEntity);
		}
		else if (ContextLoot[0].ContainsSlot(to))
		{
			m_LootItemsByObjectType[LootObjectType.Normal].Add(from.ItemEntity);
		}
		InventoryHelper.TryMoveSlot(from, to, InteractionSlot);
	}

	void ICollectLootHandler.HandleCollectAll(ItemsCollection from, ItemsCollection to)
	{
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		MarkContextLootAsDirty();
		UpdateContextLoot();
		InventoryStash.CollectionChanged();
	}

	void ITransferItemHandler.HandleTransferItem(ItemsCollection from, ItemsCollection to)
	{
		if (m_ItemsCollections.Contains(from) || m_ItemsCollections.Contains(to))
		{
			MarkContextLootAsDirty();
		}
	}

	public void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		if (state)
		{
			Close();
		}
	}

	void ICargoStateChangedHandler.HandleCreateNewCargo(CargoEntity entity)
	{
	}

	void ICargoStateChangedHandler.HandleRemoveCargo(CargoEntity entity, bool fromMassSell)
	{
	}

	void ICargoStateChangedHandler.HandleAddItemToCargo(ItemEntity item, ItemsCollection from, CargoEntity to, int oldIndex)
	{
		MarkContextLootAsDirty();
	}

	void ICargoStateChangedHandler.HandleRemoveItemFromCargo(ItemEntity item, CargoEntity from)
	{
		MarkContextLootAsDirty();
	}

	void ISplitItemHandler.HandleSplitItem()
	{
	}

	void ISplitItemHandler.HandleAfterSplitItem(ItemEntity item)
	{
		MarkContextLootAsDirty();
	}

	void ISplitItemHandler.HandleBeforeSplitItem(ItemEntity item, ItemsCollection from, ItemsCollection to)
	{
	}

	public void HandleSelectCargo(CargoSlotVM cargoSlot)
	{
		OpenDetailed?.Execute();
	}
}
