using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Common;
using Kingmaker.Visual.Sound;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Equipment;

[TypeId("fcf558235c4e3b747933f93af7617f7c")]
public class BlueprintItemEquipmentUsable : BlueprintItemEquipment
{
	public bool RemoveFromSlotWhenNoCharges = true;

	public UsableItemType Type;

	[SerializeField]
	private int m_IdentifyDC;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryEquipSound;

	[SerializeField]
	[ValidateIsPrefab]
	private GameObject m_BeltItemPrefab;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintEquipmentEnchantmentReference[] m_Enchantments;

	public override int IdentifyDC => m_IdentifyDC;

	public override string Name
	{
		get
		{
			if (string.IsNullOrEmpty(base.Name))
			{
				ItemsStrings items = LocalizedTexts.Instance.Items;
				switch (Type)
				{
				case UsableItemType.Potion:
					return string.Concat(items.PotionPrefix, " ", Abilities.FirstOrDefault()?.Name);
				case UsableItemType.Scroll:
					return string.Concat(items.ScrollPrefix, " ", Abilities.FirstOrDefault()?.Name);
				case UsableItemType.Wand:
					return string.Concat(items.WandPrefix, " ", Abilities.FirstOrDefault()?.Name);
				}
			}
			return base.Name;
		}
	}

	public override ItemsItemType ItemType => ItemsItemType.Usable;

	public override string SubtypeName => Game.Instance.BlueprintRoot.LocalizedTexts.UsableItemTypeNames.GetText(Type);

	public override string InventoryEquipSound => m_InventoryEquipSound;

	public GameObject BeltItemPrefab => m_BeltItemPrefab;

	protected override IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		IEnumerable<BlueprintItemEnchantment> enumerable = m_Enchantments?.Dereference();
		return enumerable ?? Enumerable.Empty<BlueprintItemEnchantment>();
	}
}
