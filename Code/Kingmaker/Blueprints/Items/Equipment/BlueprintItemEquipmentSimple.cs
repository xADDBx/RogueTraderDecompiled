using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

public abstract class BlueprintItemEquipmentSimple : BlueprintItemEquipment
{
	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintEquipmentEnchantmentReference[] m_Enchantments;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

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

	protected override IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		return m_Enchantments.Dereference();
	}
}
