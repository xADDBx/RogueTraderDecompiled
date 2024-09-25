using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[TypeId("579ddce9e6b6d8e44b05e0715cc66741")]
public class BlueprintItemArmor : BlueprintItemEquipment
{
	[SerializeField]
	private BlueprintArmorTypeReference m_Type;

	[SerializeField]
	private bool m_OverrideDamageAbsorption;

	[SerializeField]
	[Range(0f, 100f)]
	[ShowIf("m_OverrideDamageAbsorption")]
	private int m_DamageAbsorption;

	[SerializeField]
	private bool m_OverrideDamageDeflection;

	[SerializeField]
	[ShowIf("m_OverrideDamageDeflection")]
	private int m_DamageDeflection;

	[SerializeField]
	private ArmorVisualParameters m_VisualParameters;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintEquipmentEnchantmentReference[] m_Enchantments;

	public BlueprintArmorType Type => m_Type?.Get();

	public override string SubtypeName => Type.TypeName;

	public override string SubtypeDescription => Type.Description;

	public override string Name
	{
		get
		{
			if (string.IsNullOrEmpty(base.Name))
			{
				return Type.DefaultName;
			}
			return base.Name;
		}
	}

	public override string Description
	{
		get
		{
			if (string.IsNullOrWhiteSpace(base.Description))
			{
				return Type.Description;
			}
			return base.Description;
		}
	}

	public override ItemsItemType ItemType => ItemsItemType.Armor;

	public override Sprite Icon
	{
		get
		{
			if (!(base.Icon == null))
			{
				return base.Icon;
			}
			return Type.Icon;
		}
	}

	public ArmorVisualParameters VisualParameters => m_VisualParameters;

	public WarhammerArmorRace RaceRestriction => Type.RaceRestriction;

	public ArmorProficiencyGroup ProficiencyGroup => Type.ProficiencyGroup;

	public WarhammerArmorCategory Category => Type.Category;

	public int DamageAbsorption
	{
		get
		{
			if (!m_OverrideDamageAbsorption)
			{
				return Type.DamageAbsorption;
			}
			return m_DamageAbsorption;
		}
	}

	public int DamageDeflection
	{
		get
		{
			if (!m_OverrideDamageDeflection)
			{
				return Type.DamageDeflection;
			}
			return m_DamageDeflection;
		}
	}

	public override string InventoryEquipSound => VisualParameters.InventoryEquipSound;

	public override string InventoryPutSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryPutSound))
			{
				return base.InventoryPutSound;
			}
			return VisualParameters.InventoryPutSound;
		}
	}

	public override string InventoryTakeSound
	{
		get
		{
			if (!string.IsNullOrEmpty(base.InventoryTakeSound))
			{
				return base.InventoryTakeSound;
			}
			return VisualParameters.InventoryTakeSound;
		}
	}

	public override KingmakerEquipmentEntity EquipmentEntity => base.EquipmentEntity ?? Type.EquipmentEntity;

	public override float Weight
	{
		get
		{
			if (!(m_Weight > 0f))
			{
				return Type.Weight;
			}
			return m_Weight;
		}
	}

	public override IEnumerable<KingmakerEquipmentEntity> EquipmentEntityAlternatives
	{
		get
		{
			if (!base.EquipmentEntityAlternatives.Any())
			{
				return Type.EquipmentEntityAlternatives.Dereference();
			}
			return base.EquipmentEntityAlternatives;
		}
	}

	protected override IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		return Type.Enchantments.Concat(m_Enchantments.EmptyIfNull().Dereference());
	}

	public override void OnEnableWithLibrary()
	{
		base.OnEnableWithLibrary();
		if (Type != null)
		{
			if (m_VisualParameters == null)
			{
				m_VisualParameters = new ArmorVisualParameters();
			}
			m_VisualParameters.Prototype = Type.VisualParameters;
		}
	}

	public Race? GetRaceRestriction()
	{
		switch (RaceRestriction)
		{
		case WarhammerArmorRace.Aeldari:
		case WarhammerArmorRace.Drukhari:
			return Race.Eldar;
		case WarhammerArmorRace.Astartes:
			return Race.Spacemarine;
		case WarhammerArmorRace.Human:
			return Race.Human;
		default:
			return null;
		}
	}
}
