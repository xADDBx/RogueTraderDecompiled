using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.QA;
using Kingmaker.UI.Models.Log.ContextFlag;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;
using Warhammer.SpaceCombat.StarshipLogic.Weapon;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[TypeId("7e7f555bf3ab41c69d10ea17764526df")]
public class ReequipAllItemsOnShip : PlayerUpgraderOnlyAction
{
	[ValidateNotNull]
	[SerializeReference]
	public AbstractUnitEvaluator Target;

	public override string GetCaption()
	{
		return "Unequip all items from the target starship";
	}

	protected override void RunActionOverride()
	{
		if (!(Target.GetValue() is StarshipEntity starshipEntity))
		{
			string message = $"[IS NOT BASE UNIT ENTITY] Game action {this}, {Target} is not BaseUnitEntity";
			if (!QAModeExceptionReporter.MaybeShowError(message))
			{
				UberDebug.LogError(message);
			}
			return;
		}
		using (ContextData<GameLogDisabled>.Request())
		{
			foreach (Warhammer.SpaceCombat.StarshipLogic.Weapon.WeaponSlot weaponSlot in starshipEntity.Hull.HullSlots.WeaponSlots)
			{
				ReequipItemInSlot(weaponSlot);
			}
			foreach (ItemSlot equipmentSlot in starshipEntity.Hull.HullSlots.EquipmentSlots)
			{
				ReequipItemInSlot(equipmentSlot);
			}
		}
	}

	private void ReequipItemInSlot(ItemSlot slot)
	{
		ItemEntity itemEntity = null;
		if (slot.HasItem && slot.CanRemoveItem())
		{
			itemEntity = slot.Item;
			slot.RemoveItem();
		}
		if (itemEntity != null && slot.CanInsertItem(itemEntity))
		{
			slot.InsertItem(itemEntity);
		}
	}
}
