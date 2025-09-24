using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateAbilityActionPointCost : RulebookEvent
{
	private const int TwoWeaponAdditionalPenaltyCost = 1;

	private readonly AbilityData m_AbilityData;

	private readonly BlueprintAbility m_BlueprintAbility;

	private int m_CostMinimum = -1;

	public int Result { get; set; }

	public int CostBonus { get; set; }

	public int CostBonusAfterMinimum { get; set; }

	public bool NoTwoWeaponFightingPenalty { get; set; }

	public int CostMinimum
	{
		get
		{
			return m_CostMinimum;
		}
		set
		{
			if (m_CostMinimum == -1)
			{
				m_CostMinimum = value;
			}
			else
			{
				m_CostMinimum = Math.Min(m_CostMinimum, value);
			}
		}
	}

	public int CostOverride { get; set; }

	public int DefaultCostOverride { get; set; }

	private int DefaultCost
	{
		get
		{
			int result = ((m_AbilityData.SettingsFromItem != null && m_AbilityData.SettingsFromItem.Ability == Blueprint) ? m_AbilityData.SettingsFromItem.AP : Blueprint.ActionPointCost);
			if (DefaultCostOverride <= 0)
			{
				return result;
			}
			return DefaultCostOverride;
		}
	}

	public BlueprintAbility Blueprint => m_AbilityData?.Blueprint ?? m_BlueprintAbility;

	[CanBeNull]
	public ItemEntityWeapon Weapon => m_AbilityData?.Weapon;

	public AbilityData AbilityData => m_AbilityData;

	public RuleCalculateAbilityActionPointCost([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		m_AbilityData = ability;
		CostOverride = -1;
		DefaultCostOverride = -1;
	}

	public RuleCalculateAbilityActionPointCost([NotNull] MechanicEntity initiator, BlueprintAbility ability)
		: base(initiator)
	{
		m_BlueprintAbility = ability;
		CostOverride = -1;
		DefaultCostOverride = -1;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		int num = ((CostOverride >= 0) ? CostOverride : Math.Max(DefaultCost + CostBonus, Math.Max(CostMinimum, 0)));
		num += CostBonusAfterMinimum;
		Result = (HasPenaltyCost() ? (num + 1) : num);
	}

	private bool HasPenaltyCost()
	{
		PartTwoWeaponFighting twoWeaponFightingOptional = m_AbilityData.Caster.GetTwoWeaponFightingOptional();
		ItemEntityWeapon weapon = m_AbilityData.Weapon;
		if (twoWeaponFightingOptional != null && twoWeaponFightingOptional.EnableAttackWithPairedWeapon && twoWeaponFightingOptional.IsOtherAbilityGroupOnCooldown(m_AbilityData) && !NoTwoWeaponFightingPenalty && weapon != null && !weapon.HoldInTwoHands && !m_AbilityData.IsFreeAction && m_AbilityData.Blueprint.Type == AbilityType.Weapon && !m_AbilityData.IsBonusUsage && !m_AbilityData.IsHeroicAct)
		{
			return !m_AbilityData.IsDesperateMeasure;
		}
		return false;
	}
}
