using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.Items;
using Kingmaker.UI.Common;
using Owlcat.Runtime.UI.MVVM;
using UniRx;

namespace Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;

public class InventoryEquipSelectorVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	private readonly EquipSlotVM m_EquipSlot;

	private readonly List<ItemSlotVM> m_VisibleCollection;

	private readonly ReactiveProperty<ItemSlotVM> m_SelectedItem = new ReactiveProperty<ItemSlotVM>();

	private readonly Action m_OnClose;

	private ItemsCollection ItemsCollection => Game.Instance.Player.Inventory;

	public bool HasVisibleItems => m_VisibleCollection.Any();

	public InventoryEquipSelectorVM(Action onClose, EquipSlotVM equipSlotVM)
	{
		m_OnClose = onClose;
		m_EquipSlot = equipSlotVM;
		ItemsItemType[] itemTypesFilter = GetItemType(m_EquipSlot.SlotType);
		m_VisibleCollection = (from item in ItemsCollection
			where item.IsInStash && itemTypesFilter != null && itemTypesFilter.Contains(item.Blueprint.ItemType) && UIUtilityItem.GetEquipPosibility(item)[0]
			select new ItemSlotVM(item, 0)).ToList();
	}

	private ItemsItemType[] GetItemType(EquipSlotType equipSlotSlotType)
	{
		return equipSlotSlotType switch
		{
			EquipSlotType.PrimaryHand => new ItemsItemType[1], 
			EquipSlotType.SecondaryHand => new ItemsItemType[2]
			{
				ItemsItemType.Weapon,
				ItemsItemType.Shield
			}, 
			EquipSlotType.Armor => new ItemsItemType[1] { ItemsItemType.Armor }, 
			EquipSlotType.Belt => new ItemsItemType[1] { ItemsItemType.Belt }, 
			EquipSlotType.Head => new ItemsItemType[1] { ItemsItemType.Head }, 
			EquipSlotType.Feet => new ItemsItemType[1] { ItemsItemType.Feet }, 
			EquipSlotType.Gloves => new ItemsItemType[1] { ItemsItemType.Gloves }, 
			EquipSlotType.Neck => new ItemsItemType[1] { ItemsItemType.Neck }, 
			EquipSlotType.Ring1 => new ItemsItemType[1] { ItemsItemType.Ring }, 
			EquipSlotType.Ring2 => new ItemsItemType[1] { ItemsItemType.Ring }, 
			EquipSlotType.Wrist => new ItemsItemType[1] { ItemsItemType.Wrist }, 
			EquipSlotType.Shoulders => new ItemsItemType[1] { ItemsItemType.Shoulders }, 
			EquipSlotType.QuickSlot1 => new ItemsItemType[1] { ItemsItemType.Usable }, 
			EquipSlotType.QuickSlot2 => new ItemsItemType[1] { ItemsItemType.Usable }, 
			EquipSlotType.QuickSlot3 => new ItemsItemType[1] { ItemsItemType.Usable }, 
			EquipSlotType.QuickSlot4 => new ItemsItemType[1] { ItemsItemType.Usable }, 
			EquipSlotType.QuickSlot5 => new ItemsItemType[1] { ItemsItemType.Usable }, 
			_ => throw new ArgumentOutOfRangeException("equipSlotSlotType", equipSlotSlotType, null), 
		};
	}

	protected override void DisposeImplementation()
	{
	}
}
