using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Localization;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Armors;

[TypeId("7f17557ad5ceda141986b74c2ad0fa68")]
public class BlueprintArmorType : BlueprintScriptableObject
{
	[SerializeField]
	private LocalizedString m_TypeNameText;

	[SerializeField]
	private LocalizedString m_DefaultNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private ArmorVisualParameters m_VisualParameters;

	[SerializeField]
	private WarhammerArmorRace m_RaceRestriction;

	[SerializeField]
	private WarhammerArmorCategory m_Category;

	[SerializeField]
	private ArmorProficiencyGroup m_ProficiencyGroup;

	[SerializeField]
	[Range(0f, 100f)]
	private int m_DamageAbsorption;

	[SerializeField]
	private int m_DamageDeflection;

	[SerializeField]
	private float m_Weight;

	[SerializeField]
	private KingmakerEquipmentEntityReference m_EquipmentEntity;

	[SerializeField]
	private KingmakerEquipmentEntityReference[] m_EquipmentEntityAlternatives = new KingmakerEquipmentEntityReference[0];

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintArmorEnchantmentReference[] m_Enchantments;

	[SerializeField]
	private int m_ForcedRampColorPresetIndex;

	[SerializeField]
	[Range(-1f, 100f)]
	private int m_DodgeArmorPercentPenalty = 25;

	public LocalizedString TypeName => m_TypeNameText;

	public LocalizedString DefaultName => m_DefaultNameText;

	public LocalizedString Description => m_DescriptionText;

	public Sprite Icon => m_Icon;

	public ArmorVisualParameters VisualParameters => m_VisualParameters;

	public WarhammerArmorRace RaceRestriction => m_RaceRestriction;

	public ArmorProficiencyGroup ProficiencyGroup => m_ProficiencyGroup;

	public WarhammerArmorCategory Category => m_Category;

	public IEnumerable<BlueprintItemEnchantment> Enchantments => m_Enchantments.EmptyIfNull().Dereference();

	public KingmakerEquipmentEntity EquipmentEntity => m_EquipmentEntity.Get();

	public int ForcedRampColorPresetIndex
	{
		get
		{
			return m_ForcedRampColorPresetIndex;
		}
		set
		{
			m_ForcedRampColorPresetIndex = value;
		}
	}

	public KingmakerEquipmentEntityReference[] EquipmentEntityAlternatives => m_EquipmentEntityAlternatives;

	public int DamageAbsorption => m_DamageAbsorption;

	public int DamageDeflection => m_DamageDeflection;

	public int DodgeArmorPercentPenalty => m_DodgeArmorPercentPenalty;

	public float Weight => m_Weight;
}
