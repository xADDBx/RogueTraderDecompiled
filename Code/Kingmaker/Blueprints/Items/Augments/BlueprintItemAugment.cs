using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Augments;

[TypeId("b382345b15a445cebe88da7d988e65ab")]
public class BlueprintItemAugment : BlueprintItemEquipment
{
	[SerializeField]
	private BlueprintAugmentSlotReference m_AugmentSlot;

	[SerializeField]
	private BlueprintAbilityReference m_OverdriveAbility;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	public BlueprintAugmentSlot AugmentSlot => m_AugmentSlot;

	public BlueprintAbility OverdriveAbility => m_OverdriveAbility;

	public override ItemsItemType ItemType => ItemsItemType.Augment;

	public override string InventoryEquipSound
	{
		get
		{
			return m_InventoryEquipSound;
		}
		set
		{
			m_InventoryEquipSound = value;
		}
	}

	public override bool GainAbility
	{
		get
		{
			if (OverdriveAbility == null)
			{
				return base.GainAbility;
			}
			return true;
		}
	}

	public BlueprintItemAugment()
	{
		SpendCharges = false;
	}
}
