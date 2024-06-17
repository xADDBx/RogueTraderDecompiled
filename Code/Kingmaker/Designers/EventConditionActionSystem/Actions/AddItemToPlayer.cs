using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.UnitLogic;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Serializable]
[AllowMultipleComponents]
[PlayerUpgraderAllowed(false)]
[TypeId("1f1c883ef085c5c4b877eeb4c5da318c")]
public class AddItemToPlayer : GameAction
{
	private enum HandType
	{
		Any,
		Primary,
		Secondary
	}

	[ValidateNotNull]
	[SerializeField]
	private BlueprintItemReference m_ItemToGive;

	public bool Silent;

	public int Quantity = 1;

	public bool Identify;

	[ShowIf("IsEquippable")]
	public bool Equip;

	[SerializeReference]
	[ShowIf("Equip")]
	public AbstractUnitEvaluator EquipOn;

	[ShowIf("IsEquippableIntoWeaponSlot")]
	[Tooltip("Select weaponset number for a weapon. 0 means first available, 1-2 to select specific slot")]
	public int PreferredWeaponSet;

	[SerializeField]
	[ShowIf("IsPreferredWeaponSlotSet")]
	private HandType m_Hand;

	[ShowIf("Equip")]
	public bool ForceEquip;

	[ShowIf("Equip")]
	public bool ErrorIfDidNotEquip = true;

	public BlueprintItem ItemToGive => m_ItemToGive?.Get();

	private bool IsEquippable
	{
		get
		{
			if (Quantity == 1)
			{
				return ItemToGive is BlueprintItemEquipment;
			}
			return false;
		}
	}

	private bool IsEquippableIntoWeaponSlot
	{
		get
		{
			if (IsEquippable && ItemToGive is BlueprintItemEquipmentHand)
			{
				return Equip;
			}
			return false;
		}
	}

	private bool IsPreferredWeaponSlotSet
	{
		get
		{
			if (IsEquippableIntoWeaponSlot)
			{
				return PreferredWeaponSet > 0;
			}
			return false;
		}
	}

	public override string GetDescription()
	{
		return "Добавляет игроку указанные предметы.\nПредметы можно сразу идентифицировать\nОдеваемые предметы можно одеть на указанного юнита";
	}

	public override void RunAction()
	{
		if ((bool)ContextData<UnitHelper.DoNotCreateItems>.Current)
		{
			return;
		}
		using (ContextData<GameLogDisabled>.RequestIf(Silent))
		{
			ItemsCollection inventory = Game.Instance.Player.Inventory;
			if (ItemToGive.IsActuallyStackable)
			{
				ItemEntity itemEntity = ItemToGive.CreateEntity();
				if (Quantity > 1)
				{
					itemEntity.IncrementCount(Quantity - 1, inventory.ForceStackable);
				}
				if (Identify)
				{
					itemEntity.Identify();
				}
				itemEntity = inventory.Add(itemEntity);
				if (Equip)
				{
					EquipItem(itemEntity);
				}
				return;
			}
			for (int i = 0; i < Quantity; i++)
			{
				ItemEntity itemEntity2 = ItemToGive.CreateEntity();
				if (Identify)
				{
					itemEntity2.Identify();
				}
				itemEntity2 = inventory.Add(itemEntity2);
				if (Equip)
				{
					EquipItem(itemEntity2);
				}
			}
		}
	}

	private void EquipItem(ItemEntity item)
	{
		using (ContextData<ItemSlot.IgnoreLock>.RequestIf(ForceEquip))
		{
			BaseUnitEntity baseUnitEntity = ((EquipOn == null) ? GameHelper.GetPlayerCharacter() : ((BaseUnitEntity)EquipOn.GetValue()));
			if (baseUnitEntity == null)
			{
				return;
			}
			PartUnitBody body = baseUnitEntity.Body;
			ItemSlot itemSlot;
			if (IsPreferredWeaponSlotSet)
			{
				HandsEquipmentSet handsEquipmentSet = body.HandsEquipmentSets[PreferredWeaponSet - 1];
				HandSlot primaryHand = handsEquipmentSet.PrimaryHand;
				HandSlot secondaryHand = handsEquipmentSet.SecondaryHand;
				switch (m_Hand)
				{
				case HandType.Any:
				{
					bool hasItem = primaryHand.HasItem;
					bool num = primaryHand.CanInsertItem(item) && (!hasItem || primaryHand.CanRemoveItem());
					bool hasItem2 = secondaryHand.HasItem;
					bool flag = secondaryHand.CanInsertItem(item) && (!hasItem2 || secondaryHand.CanRemoveItem());
					itemSlot = ((num && (!hasItem || (hasItem2 && !flag))) ? primaryHand : ((!flag) ? null : secondaryHand));
					break;
				}
				case HandType.Primary:
					itemSlot = primaryHand;
					break;
				case HandType.Secondary:
					itemSlot = secondaryHand;
					break;
				default:
					throw new ArgumentOutOfRangeException();
				}
			}
			else if (item is ItemEntityMechadendrite)
			{
				itemSlot = body.EquipmentSlots.FirstOrDefault((ItemSlot s) => s.CanInsertItem(item) && (!s.HasItem || s.CanRemoveItem()) && s.MaybeItem?.Blueprint == item.Blueprint);
				if (itemSlot == null)
				{
					EquipmentSlot<BlueprintItemMechadendrite> equipmentSlot = new EquipmentSlot<BlueprintItemMechadendrite>(baseUnitEntity);
					baseUnitEntity.Body.EquipmentSlots.Add(equipmentSlot);
					baseUnitEntity.Body.AllSlots.Add(equipmentSlot);
					baseUnitEntity.Body.Mechadendrites.Add(equipmentSlot);
					itemSlot = equipmentSlot;
				}
			}
			else
			{
				itemSlot = body.EquipmentSlots.FirstOrDefault((ItemSlot s) => s.CanInsertItem(item) && !s.HasItem) ?? body.EquipmentSlots.FirstOrDefault((ItemSlot s) => s.CanInsertItem(item) && s.HasItem && s.CanRemoveItem());
			}
			if (itemSlot != null)
			{
				if (itemSlot.HasItem)
				{
					itemSlot.RemoveItem();
				}
				itemSlot.InsertItem(item);
			}
			if (ErrorIfDidNotEquip && itemSlot?.MaybeItem == null)
			{
				PFLog.Default.ErrorWithReport(base.Owner, $"{base.Owner}: Cannot equip {item} on {baseUnitEntity}: not slot available");
			}
		}
	}

	public override string GetCaption()
	{
		if (Quantity != 1)
		{
			return $"Add Item ({ItemToGive} x{Quantity}) to player";
		}
		return $"Add Item ({ItemToGive}) to player";
	}
}
