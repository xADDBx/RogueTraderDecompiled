using System;
using Kingmaker.Blueprints.Items.Augments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM.VM.ServiceWindows.Inventory;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Items.Slots;
using Kingmaker.Utility.Attributes;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("326aea5bb8239cd429a9a4734423189d")]
public class CheckAbilitySourceIsItemInCertainSlotGetter : PropertyGetter, PropertyContextAccessor.IAbility, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	[SerializeField]
	private EquipSlotType m_ItemSlotType;

	[SerializeField]
	[ShowIf("m_IsItemAugment")]
	private BlueprintAugmentSlotReference m_AugmentSlot;

	private bool m_IsItemAugment => m_ItemSlotType == EquipSlotType.Augment;

	protected override int GetBaseValue()
	{
		ItemSlot itemSlot = this.GetAbility()?.SourceItem?.HoldingSlot;
		if (itemSlot == null)
		{
			return 0;
		}
		if (m_IsItemAugment)
		{
			if ((itemSlot as AugmentSlot)?.Blueprint != m_AugmentSlot.Get())
			{
				return 0;
			}
			return 1;
		}
		if (!MatchesEquipmentSlot(itemSlot))
		{
			return 0;
		}
		return 1;
	}

	private bool MatchesEquipmentSlot(ItemSlot slot)
	{
		switch (m_ItemSlotType)
		{
		case EquipSlotType.PrimaryHand:
		case EquipSlotType.SecondaryHand:
			return slot is HandSlot;
		case EquipSlotType.Armor:
			return slot is ArmorSlot;
		case EquipSlotType.Belt:
			return slot is EquipmentSlot<BlueprintItemEquipmentBelt>;
		case EquipSlotType.Head:
			return slot is EquipmentSlot<BlueprintItemEquipmentHead>;
		case EquipSlotType.Feet:
			return slot is EquipmentSlot<BlueprintItemEquipmentFeet>;
		case EquipSlotType.Gloves:
			return slot is EquipmentSlot<BlueprintItemEquipmentGloves>;
		case EquipSlotType.Neck:
			return slot is EquipmentSlot<BlueprintItemEquipmentNeck>;
		case EquipSlotType.Ring1:
		case EquipSlotType.Ring2:
			return slot is EquipmentSlot<BlueprintItemEquipmentRing>;
		case EquipSlotType.Wrist:
			return slot is EquipmentSlot<BlueprintItemEquipmentWrist>;
		case EquipSlotType.Shoulders:
			return slot is EquipmentSlot<BlueprintItemEquipmentShoulders>;
		case EquipSlotType.PetProtocol:
			return slot is EquipmentSlot<BlueprintItemEquipmentPetProtocol>;
		case EquipSlotType.Glasses:
			return slot is EquipmentSlot<BlueprintItemEquipmentGlasses>;
		case EquipSlotType.Shirt:
			return slot is EquipmentSlot<BlueprintItemEquipmentShirt>;
		case EquipSlotType.QuickSlot1:
		case EquipSlotType.QuickSlot2:
		case EquipSlotType.QuickSlot3:
		case EquipSlotType.QuickSlot4:
		case EquipSlotType.QuickSlot5:
			return slot is UsableSlot;
		default:
			return false;
		}
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check that ability source is item in a certain slot: {m_ItemSlotType}";
	}
}
