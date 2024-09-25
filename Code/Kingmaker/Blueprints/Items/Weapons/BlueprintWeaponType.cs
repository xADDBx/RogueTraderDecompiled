using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Components;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Localization;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Weapons;

[TypeId("51210e03e441ea249be955610f84c748")]
public class BlueprintWeaponType : BlueprintScriptableObject, IUIDataProvider
{
	public BlueprintBuffReference[] warhammerHitEffects;

	public BlueprintBuffReference[] warhammerCritEffects;

	public WeaponCategory Category;

	public WeaponFamily Family;

	[SerializeField]
	private LocalizedString m_TypeNameText;

	[SerializeField]
	private LocalizedString m_DefaultNameText;

	[SerializeField]
	private LocalizedString m_DescriptionText;

	[SerializeField]
	private Sprite m_Icon;

	[SerializeField]
	private WeaponVisualParameters m_VisualParameters;

	[SerializeField]
	private DamageTypeDescription m_DamageType;

	[SerializeField]
	private bool m_IsNatural;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintWeaponEnchantmentReference[] m_Enchantments;

	[SerializeField]
	private DamageStatBonusFactor m_DamageStatBonusFactor;

	public LocalizedString TypeName => m_TypeNameText;

	public LocalizedString DefaultName => m_DefaultNameText;

	public LocalizedString Description => m_DescriptionText;

	public Sprite Icon => m_Icon;

	public IEnumerable<BlueprintItemEnchantment> Enchantments => m_Enchantments.Dereference();

	public WeaponVisualParameters VisualParameters => m_VisualParameters;

	public bool IsNatural => m_IsNatural;

	public DamageTypeDescription DamageType => m_DamageType;

	public int? OverrideOverpenetrationFactorPercents => this.GetComponent<OverrideOverpenetrationFactor>()?.FactorPercents;

	public DamageStatBonusFactor DamageStatBonusFactor => m_DamageStatBonusFactor;

	string IUIDataProvider.Name => DefaultName;

	string IUIDataProvider.Description => Description;

	Sprite IUIDataProvider.Icon => Icon;

	string IUIDataProvider.NameForAcronym => name;
}
