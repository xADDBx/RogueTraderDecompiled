using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Items.Armors;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UI.Common;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Blueprints.Items.Shields;

[TypeId("9ed5cf7385012e747b3d2e8f9d1ac6b8")]
public class BlueprintItemShield : BlueprintItemEquipmentHand
{
	[SerializeField]
	[JsonProperty(PropertyName = "AbilityContainer")]
	public WeaponAbilityContainer WeaponAbilities;

	[SerializeField]
	private WarhammerArmorCategory m_Category;

	[SerializeField]
	[Tooltip("Базовый шанс блока")]
	private int m_BlockChance;

	[SerializeField]
	[Tooltip("Опциональный оружейны компонент, для нанесения урона")]
	private BlueprintItemWeaponReference m_WeaponComponent;

	[SerializeField]
	[Tooltip("После первого блока скаттер шот выстрела, блокировать все следующие попадания проджектайлов этого выстрела?")]
	private bool m_EnableScatterAutoBlockAfterFirstBlock;

	[SerializeField]
	[ValidateNoNullEntries]
	private BlueprintEquipmentEnchantmentReference[] m_Enchantments;

	[CanBeNull]
	public BlueprintItemWeapon WeaponComponent => m_WeaponComponent?.Get();

	public WarhammerArmorCategory Category => m_Category;

	public int BlockChance => m_BlockChance;

	public bool EnableScatterAutoBlockAfterFirstBlock => m_EnableScatterAutoBlockAfterFirstBlock;

	public override string SubtypeName => Game.Instance.BlueprintRoot.LocalizedTexts.UnidentifiedItemNames.GetText(ItemsItemType.Shield);

	public override ItemsItemType ItemType => ItemsItemType.Shield;

	public override IEnumerable<BlueprintAbility> Abilities => base.Abilities.Concat(WeaponAbilities.Select((WeaponAbility i) => i.Ability).NotNull());

	public override bool GainAbility
	{
		get
		{
			if (!base.GainAbility)
			{
				return WeaponAbilities.Any();
			}
			return true;
		}
	}

	protected override IEnumerable<BlueprintItemEnchantment> CollectEnchantments()
	{
		return m_Enchantments.EmptyIfNull().Dereference().Concat((WeaponComponent?.Enchantments).EmptyIfNull());
	}
}
