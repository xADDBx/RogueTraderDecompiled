using System;
using System.Collections.Generic;
using System.Linq;
using Code.Enums;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.Attributes;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Blueprints.Items;

[TypeId("bdd0ca0d56a2ac5479e67a3f2bda917f")]
public class BlueprintItem : BlueprintMechanicEntityFact
{
	public enum MiscellaneousItemType
	{
		None,
		Jewellery,
		Gems,
		AnimalParts
	}

	public enum ItemRarity
	{
		Trash,
		Lore,
		Common,
		Pattern,
		Unique,
		Quest
	}

	[SerializeField]
	private LocalizedString m_FlavorText;

	[SerializeField]
	[StringCreateTemplate(StringCreateTemplateAttribute.StringType.ItemText)]
	private LocalizedString m_NonIdentifiedNameText;

	[SerializeField]
	private LocalizedString m_NonIdentifiedDescriptionText;

	[SerializeField]
	[ShowIf("AllowMakeStackable")]
	private bool m_NotStackable;

	[SerializeField]
	protected float m_Weight;

	[SerializeField]
	private ItemsItemOrigin m_Origin = ItemsItemOrigin.Miscellaneous;

	[SerializeField]
	private ItemRarity m_Rarity = ItemRarity.Common;

	[SerializeField]
	private ItemTag m_Tag;

	[SerializeField]
	private BlueprintItemPatternReference m_Pattern;

	[SerializeField]
	private bool m_IsNotable;

	[SerializeField]
	private float m_ProfitFactorCost;

	[SerializeField]
	[Range(0f, 100f)]
	private int m_CargoVolumePercent = 10;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryPutSound;

	[SerializeField]
	[AkEventReference]
	private string m_InventoryTakeSound;

	private List<BlueprintItemEnchantment> m_CachedEnchantments;

	public bool ToCargoAutomatically;

	public int ItemLevel;

	public virtual float Weight => m_Weight;

	public ItemsItemOrigin Origin => m_Origin;

	public bool IsNotable => m_IsNotable;

	public float ProfitFactorCost => m_ProfitFactorCost;

	public int CargoVolumePercent => m_CargoVolumePercent;

	public ItemRarity Rarity => m_Rarity;

	public ItemTag Tag => m_Tag;

	protected virtual bool AllowMakeStackable => true;

	public virtual string FlavorText => m_FlavorText;

	public virtual string SubtypeName => "";

	public virtual string SubtypeDescription => "";

	public virtual ItemsItemType ItemType => ItemsItemType.NonUsable;

	public bool IsActuallyStackable
	{
		get
		{
			if (AllowMakeStackable)
			{
				return !m_NotStackable;
			}
			return false;
		}
	}

	public List<BlueprintItemEnchantment> Enchantments
	{
		get
		{
			if (!Application.isPlaying)
			{
				return CollectEnchantments().ToList();
			}
			return m_CachedEnchantments ?? (m_CachedEnchantments = CollectEnchantments().ToList());
		}
	}

	public virtual int IdentifyDC
	{
		get
		{
			if (!Enchantments.Any())
			{
				return 0;
			}
			return Enchantments.Max((BlueprintItemEnchantment e) => e?.IdentifyDC ?? 0);
		}
	}

	public virtual string InventoryPutSound => m_InventoryPutSound;

	public virtual string InventoryTakeSound => m_InventoryTakeSound;

	public override void OnEnable()
	{
		base.OnEnable();
		m_CachedEnchantments = null;
	}

	protected virtual IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		return Enumerable.Empty<BlueprintItemEnchantment>();
	}

	public override string ToString()
	{
		return "[" + GetType().Name + ": " + base.ToString() + "]";
	}

	protected override Type GetFactType()
	{
		return GetType();
	}

	public ItemQuality GetItemQuality()
	{
		int cargoVolumePercent = CargoVolumePercent;
		if (cargoVolumePercent > 10)
		{
			if (cargoVolumePercent <= 20)
			{
				return ItemQuality.Middle;
			}
			return ItemQuality.Expensive;
		}
		return ItemQuality.Cheap;
	}
}
