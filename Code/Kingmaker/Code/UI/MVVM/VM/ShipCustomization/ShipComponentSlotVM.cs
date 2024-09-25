using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Code.UI.MVVM.VM.Slots;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameCommands;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities;
using UniRx;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Code.UI.MVVM.VM.ShipCustomization;

public class ShipComponentSlotVM : ItemSlotVM, IInsertItemHandler, ISubscriber, IUnequipItemHandler, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, IMoveItemHandler, IEquipSlotPossibleTarget, ISubscriber<ItemEntity>, IEquipSlotHoverHandler
{
	public readonly ShipComponentSlotType SlotType;

	public readonly WeaponSlotType WeaponSlotType;

	private readonly ReactiveProperty<string> m_SlotDescription = new ReactiveProperty<string>(string.Empty);

	public readonly ReactiveProperty<bool> NeedRepair = new ReactiveProperty<bool>(initialValue: false);

	public new readonly ReactiveProperty<bool> PossibleTarget = new ReactiveProperty<bool>(initialValue: false);

	public readonly ReactiveProperty<bool> ShowPossibleTarget = new ReactiveProperty<bool>(initialValue: true);

	public readonly PlayerShipType ShipType;

	public readonly BoolReactiveProperty IsLocked = new BoolReactiveProperty();

	public readonly BoolReactiveProperty HasArsenalAdvancement = new BoolReactiveProperty();

	private bool m_IsPossibleHighlighted;

	private bool m_IsPossibleHovered;

	public int SetIndex { get; }

	public ItemSlot ItemSlot { get; }

	public ShipComponentSlotVM(ShipComponentSlotType slotType, ItemSlot itemSlot, int index = -1, WeaponSlotType weaponSlotType = WeaponSlotType.None, bool isLocked = false)
		: base(itemSlot.MaybeItem, index)
	{
		SlotType = slotType;
		WeaponSlotType = weaponSlotType;
		ItemSlot = itemSlot;
		SetIndex = index;
		IsLocked.Value = isLocked;
		ShipType = Game.Instance.Player.PlayerShip.Blueprint.ShipType;
		m_SlotDescription.Value = GetSlotDescription();
		UpdateArsenalAdvancement();
	}

	public bool IsPossibleTarget(ItemEntity item)
	{
		if (ItemSlot.CanInsertItem(item) && (!ItemSlot.HasItem || ItemSlot.CanRemoveItem()))
		{
			return CanHighLight(item);
		}
		return false;
	}

	private bool CanHighLight(ItemEntity item)
	{
		if (WeaponSlotType == WeaponSlotType.None)
		{
			return true;
		}
		BlueprintStarshipWeapon blueprintStarshipWeapon = item.Blueprint as BlueprintStarshipWeapon;
		if (!blueprintStarshipWeapon)
		{
			return true;
		}
		return blueprintStarshipWeapon?.AllowedSlots.Contains(WeaponSlotType) ?? false;
	}

	public void InsertItem(ItemEntity item)
	{
		if (!IsLocked.Value && ItemSlot != null && ItemSlot.CanInsertItem(item))
		{
			Game.Instance.GameCommandQueue.EquipItem(item, ItemSlot.Owner, this.ToSlotRef());
		}
	}

	private string GetSlotDescription()
	{
		UIShipCustomization shipCustomization = UIStrings.Instance.ShipCustomization;
		return SlotType switch
		{
			ShipComponentSlotType.PlasmaDrives => shipCustomization.PlasmaDrives, 
			ShipComponentSlotType.VoidShieldGenerator => shipCustomization.VoidShieldGenerator, 
			ShipComponentSlotType.AugerArray => shipCustomization.AugerArray, 
			ShipComponentSlotType.ArmorPlating => shipCustomization.ArmorPlating, 
			ShipComponentSlotType.Dorsal => shipCustomization.ShipWeapon, 
			ShipComponentSlotType.Prow1 => shipCustomization.ShipWeapon, 
			ShipComponentSlotType.Prow2 => shipCustomization.ShipWeapon, 
			ShipComponentSlotType.Starboard => shipCustomization.ShipWeapon, 
			ShipComponentSlotType.Arsenal => shipCustomization.Arsenal, 
			_ => string.Empty, 
		};
	}

	void IInsertItemHandler.HandleInsertItem(ItemSlot slot)
	{
		RefreshItem();
	}

	void IUnitEquipmentHandler.HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		RefreshItem();
	}

	void IUnequipItemHandler.HandleUnequipItem()
	{
		RefreshItem();
	}

	void IMoveItemHandler.HandleMoveItem(bool isEquip)
	{
		if (isEquip)
		{
			RefreshItem();
		}
	}

	private void RefreshItem()
	{
		Item.Value = ItemSlot.MaybeItem;
		UpdateArsenalAdvancement();
	}

	public void HandleHighlightStart(ItemEntity item)
	{
		m_IsPossibleHighlighted = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public new void HandleHighlightStop()
	{
		m_IsPossibleHighlighted = false;
		UpdatePossibleTarget();
	}

	private void UpdatePossibleTarget()
	{
		PossibleTarget.Value = m_IsPossibleHighlighted || m_IsPossibleHovered;
	}

	public void HandleHoverStart(ItemEntity item)
	{
		m_IsPossibleHovered = IsPossibleTarget(item);
		UpdatePossibleTarget();
	}

	public new void HandleHoverStop()
	{
		m_IsPossibleHovered = false;
		UpdatePossibleTarget();
	}

	public void SetPossibleTargetState(bool state)
	{
		ShowPossibleTarget.Value = state;
	}

	private void UpdateArsenalAdvancement()
	{
		if (ItemSlot.MaybeItem != null && ItemSlot is Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot)
		{
			BoolReactiveProperty hasArsenalAdvancement = HasArsenalAdvancement;
			IEnumerable<Ability> abilityVariants = weaponSlot.AbilityVariants;
			hasArsenalAdvancement.Value = abilityVariants != null && abilityVariants.Where((Ability a) => a != null).Count() > 1;
		}
		else
		{
			HasArsenalAdvancement.Value = false;
		}
	}
}
