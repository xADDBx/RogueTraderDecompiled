using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.Items;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[TypeId("482ed190354e4d448017d3088e7c91d0")]
public class ContextConditionEquipmentSlot : ContextCondition
{
	public EquipSlotType EquipmentSlot;

	[SerializeField]
	private bool IsCaster;

	protected override string GetConditionCaption()
	{
		if (!IsCaster)
		{
			return "Check if target has equipment slot filled";
		}
		return "Check if caster has equipment slot filled";
	}

	protected override bool CheckCondition()
	{
		PartUnitBody partUnitBody = ((!IsCaster) ? base.Target.Entity?.GetBodyOptional() : base.Context.MaybeOwner?.GetBodyOptional());
		if (partUnitBody == null)
		{
			return false;
		}
		return EquipmentSlot switch
		{
			EquipSlotType.PrimaryHand => partUnitBody.PrimaryHand.HasItem, 
			EquipSlotType.SecondaryHand => partUnitBody.SecondaryHand.HasItem, 
			EquipSlotType.Armor => partUnitBody.Armor.HasItem, 
			EquipSlotType.Belt => partUnitBody.Belt.HasItem, 
			EquipSlotType.Head => partUnitBody.Head.HasItem, 
			EquipSlotType.Feet => partUnitBody.Feet.HasItem, 
			EquipSlotType.Gloves => partUnitBody.Gloves.HasItem, 
			EquipSlotType.Neck => partUnitBody.Neck.HasItem, 
			EquipSlotType.Ring1 => partUnitBody.Ring1.HasItem, 
			EquipSlotType.Ring2 => partUnitBody.Ring2.HasItem, 
			EquipSlotType.Wrist => partUnitBody.Wrist.HasItem, 
			EquipSlotType.Shoulders => partUnitBody.Shoulders.HasItem, 
			EquipSlotType.Glasses => partUnitBody.Glasses.HasItem, 
			EquipSlotType.Shirt => partUnitBody.Shirt.HasItem, 
			EquipSlotType.QuickSlot1 => partUnitBody.QuickSlots[0].HasItem, 
			EquipSlotType.QuickSlot2 => partUnitBody.QuickSlots[1].HasItem, 
			EquipSlotType.QuickSlot3 => partUnitBody.QuickSlots[2].HasItem, 
			EquipSlotType.QuickSlot4 => partUnitBody.QuickSlots[3].HasItem, 
			EquipSlotType.QuickSlot5 => false, 
			_ => false, 
		};
	}
}
