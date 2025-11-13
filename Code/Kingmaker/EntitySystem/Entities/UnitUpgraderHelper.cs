using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Entities;

public static class UnitUpgraderHelper
{
	public static void ApplyUpgraders(BaseUnitEntity unit, BlueprintUnit blueprint, bool fromPlaceholder, ref List<BlueprintUnitUpgrader> appliedUpgraders)
	{
		IEnumerable<BlueprintUnitUpgrader> enumerable = blueprint.GetComponent<UnitUpgraderComponent>()?.EnumerateUpgraders(fromPlaceholder);
		if (enumerable != null)
		{
			foreach (BlueprintUnitUpgrader item in enumerable)
			{
				if (!appliedUpgraders.HasItem(item))
				{
					try
					{
						item.Upgrade(unit);
					}
					catch (Exception exception)
					{
						PFLog.Default.ExceptionWithReport(exception, null);
					}
					appliedUpgraders = appliedUpgraders ?? new List<BlueprintUnitUpgrader>();
					appliedUpgraders.Add(item);
				}
			}
		}
		ApplyFixes(unit);
	}

	public static void SetAllUpgradersApplied(BlueprintUnit blueprint, bool fromPlaceholder, ref List<BlueprintUnitUpgrader> appliedUpgraders)
	{
		IEnumerable<BlueprintUnitUpgrader> enumerable = blueprint.GetComponent<UnitUpgraderComponent>()?.EnumerateUpgraders(fromPlaceholder);
		if (enumerable != null && enumerable.Any())
		{
			appliedUpgraders = new List<BlueprintUnitUpgrader>(enumerable);
		}
	}

	private static void ApplyFixes(BaseUnitEntity unit)
	{
		bool flag;
		switch (unit.GetCompanionOptional()?.State)
		{
		case CompanionState.Remote:
		case CompanionState.InParty:
		case CompanionState.InPartyDetached:
			flag = true;
			break;
		default:
			flag = false;
			break;
		}
		if (flag)
		{
			unit.UnequipItemsWithFailedRestrictions();
		}
		RemoveMissingAbilities(unit);
	}

	private static void RemoveMissingAbilities(BaseUnitEntity unit)
	{
		try
		{
			foreach (Ability item in unit.Abilities.RawFacts.ToTempList())
			{
				if (!(item.FirstSource != null) || item.FirstSource.IsMissing)
				{
					unit.Abilities.Remove(item);
				}
			}
			RemoveBrokenAbilities(unit.Body.SecondaryHand.MaybeItem);
			RemoveBrokenAbilities(unit.Body.PrimaryHand.MaybeItem);
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
		void RemoveBrokenAbilities(ItemEntity item)
		{
			if (item == null)
			{
				return;
			}
			foreach (Ability item2 in item.Abilities.ToTempList())
			{
				if (item2.Owner == null)
				{
					PFLog.Ability.Log($"Ability removed from {item} on {unit}, because  it is broken");
					item.Abilities.Remove(item2);
				}
			}
		}
	}

	public static void UnequipItemsWithFailedRestrictions(this BaseUnitEntity unit)
	{
		try
		{
			if (unit.IsInCombat)
			{
				unit.CombatState.RecheckEquipmentRestrictionsAfterCombatEnd = true;
				return;
			}
			using (ContextData<ItemSlot.IgnoreLock>.Request())
			{
				foreach (ItemSlot equipmentSlot in unit.Body.EquipmentSlots)
				{
					ItemEntity maybeItem = equipmentSlot.MaybeItem;
					if (maybeItem != null && !(equipmentSlot is UsableSlot) && !equipmentSlot.CanInsertItem(maybeItem))
					{
						equipmentSlot.RemoveItem();
						PFLog.Default.Warning($"Unequipped {maybeItem} from {unit} because of restrictions");
					}
				}
				foreach (HandsEquipmentSet handsEquipmentSet in unit.Body.HandsEquipmentSets)
				{
					ItemEntityWeapon maybeWeapon = handsEquipmentSet.PrimaryHand.MaybeWeapon;
					if (maybeWeapon != null && maybeWeapon.HoldInTwoHands && handsEquipmentSet.SecondaryHand.MaybeItem != null)
					{
						ItemEntity maybeItem2 = handsEquipmentSet.SecondaryHand.MaybeItem;
						handsEquipmentSet.SecondaryHand.RemoveItem();
						PFLog.Default.Warning($"Unequipped {maybeItem2} from {unit} because of two-handed weapon in paired slot");
					}
					ItemEntityWeapon maybeWeapon2 = handsEquipmentSet.SecondaryHand.MaybeWeapon;
					if (maybeWeapon2 != null && maybeWeapon2.HoldInTwoHands && handsEquipmentSet.PrimaryHand.MaybeItem != null)
					{
						ItemEntity maybeItem3 = handsEquipmentSet.PrimaryHand.MaybeItem;
						handsEquipmentSet.PrimaryHand.RemoveItem();
						PFLog.Default.Warning($"Unequipped {maybeItem3} from {unit} because of two-handed weapon in paired slot");
					}
				}
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(ex);
		}
	}
}
