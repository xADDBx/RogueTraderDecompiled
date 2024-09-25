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

	public override string InventoryEquipSound => m_InventoryEquipSound;

	protected override IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		return m_Enchantments.Dereference();
	}
}
