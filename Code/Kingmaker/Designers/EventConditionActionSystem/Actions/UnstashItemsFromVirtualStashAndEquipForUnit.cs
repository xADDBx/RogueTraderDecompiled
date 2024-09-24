using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UI.Models.Log.ContextFlag;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("09e43bab34e941ceaa7e9827c4dc4229")]
public class UnstashItemsFromVirtualStashAndEquipForUnit : GameAction
{
	private class AvailableWeaponSet
	{
		public readonly int Index;

		private bool m_IsMainHand;

		public bool IsMainHand => m_IsMainHand;

		private AvailableWeaponSet()
		{
		}

		public AvailableWeaponSet(int index)
		{
			Index = index;
			m_IsMainHand = true;
		}

		public void SwitchToOffHand()
		{
			m_IsMainHand = false;
		}
	}

	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator EquipOn;

	public BlueprintItemsStashReference SourceStash;

	public bool Silent;

	private HashSet<ItemSlot> m_EquipSlot = new HashSet<ItemSlot>();

	public override string GetCaption()
	{
		return "Экипирует все предметы из стеша " + SourceStash?.NameSafe() + " на юнита " + EquipOn?.name + ", неподходящие предметы помещает в инвентарь";
	}

	protected override void RunAction()
	{
		if (!(EquipOn.GetValue() is BaseUnitEntity unit))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {EquipOn} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
		}
		else
		{
			if (!Game.Instance.Player.VirtualStashes.TryGetValue(SourceStash, out var value))
			{
				return;
			}
			using (ContextData<GameLogDisabled>.RequestIf(Silent))
			{
				m_EquipSlot.Clear();
				List<AvailableWeaponSet> list = new List<AvailableWeaponSet>();
				for (int i = 0; i < 2; i++)
				{
					list.Add(new AvailableWeaponSet(i));
				}
				ItemsCollection inventory = Game.Instance.Player.Inventory;
				for (int num = value.Items.Count - 1; num >= 0; num--)
				{
					ItemEntity itemEntity = value.Items[num];
					if (itemEntity != null)
					{
						value.Transfer(itemEntity, inventory);
						if (itemEntity is ItemEntityWeapon itemEntityWeapon)
						{
							if (itemEntityWeapon.HoldInTwoHands)
							{
								AvailableWeaponSet availableWeaponSet = list.FirstOrDefault((AvailableWeaponSet ws) => ws.IsMainHand);
								if (availableWeaponSet != null)
								{
									EquipWeapon(itemEntity, unit, availableWeaponSet.Index, isPrimaryHand: true);
									list.Remove(availableWeaponSet);
								}
							}
							else
							{
								AvailableWeaponSet availableWeaponSet2 = ((list.Count > 0) ? list[0] : null);
								if (availableWeaponSet2 != null)
								{
									EquipWeapon(itemEntity, unit, availableWeaponSet2.Index, availableWeaponSet2.IsMainHand);
									if (!availableWeaponSet2.IsMainHand)
									{
										list.Remove(availableWeaponSet2);
									}
									else
									{
										availableWeaponSet2.SwitchToOffHand();
									}
								}
							}
						}
						else
						{
							EquipItem(itemEntity, unit);
						}
					}
				}
				Game.Instance.Player.VirtualStashes.Remove(SourceStash);
			}
		}
	}

	private void EquipItem(ItemEntity item, BaseUnitEntity unit)
	{
		PartUnitBody body = unit.Body;
		ItemSlot itemSlot;
		if (item is ItemEntityMechadendrite)
		{
			itemSlot = body.EquipmentSlots.FirstOrDefault((ItemSlot s) => !m_EquipSlot.Contains(s) && s.CanInsertItem(item) && (!s.HasItem || s.CanRemoveItem()) && s.MaybeItem?.Blueprint == item.Blueprint);
			if (itemSlot == null)
			{
				EquipmentSlot<BlueprintItemMechadendrite> equipmentSlot = new EquipmentSlot<BlueprintItemMechadendrite>(unit);
				body.EquipmentSlots.Add(equipmentSlot);
				body.AllSlots.Add(equipmentSlot);
				body.Mechadendrites.Add(equipmentSlot);
				itemSlot = equipmentSlot;
			}
		}
		else
		{
			itemSlot = body.EquipmentSlots.FirstOrDefault((ItemSlot s) => !m_EquipSlot.Contains(s) && s.CanInsertItem(item) && !s.HasItem) ?? body.EquipmentSlots.FirstOrDefault((ItemSlot s) => !m_EquipSlot.Contains(s) && s.CanInsertItem(item) && s.HasItem && s.CanRemoveItem());
		}
		EquipItemInSlot(itemSlot, item);
	}

	private void EquipWeapon(ItemEntity item, BaseUnitEntity unit, int preferredWeaponSet, bool isPrimaryHand)
	{
		if (preferredWeaponSet < 2)
		{
			HandsEquipmentSet handsEquipmentSet = unit.Body.HandsEquipmentSets[preferredWeaponSet];
			EquipItemInSlot(isPrimaryHand ? handsEquipmentSet.PrimaryHand : handsEquipmentSet.SecondaryHand, item);
		}
	}

	private void EquipItemInSlot(ItemSlot slot, ItemEntity item)
	{
		if (slot != null)
		{
			if (slot.HasItem)
			{
				slot.RemoveItem();
			}
			slot.InsertItem(item);
			if (slot.MaybeItem != null)
			{
				m_EquipSlot.Add(slot);
			}
		}
	}
}
