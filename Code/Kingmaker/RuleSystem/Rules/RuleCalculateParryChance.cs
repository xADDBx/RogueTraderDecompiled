using System;
using System.Collections.Generic;
using Code.UnitLogic.FactLogic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Enums;

namespace Kingmaker.RuleSystem.Rules;

public class RuleCalculateParryChance : RulebookOptionalTargetEvent<UnitEntity, MechanicEntity>
{
	private int m_ResultSuperiorityNumber;

	public readonly ValueModifiersManager ParryValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager AttackerWeaponSkillValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager DefenderCurrentAttackSkillValueModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager DefenderAttackRedirectionChanceModifiers = new ValueModifiersManager();

	public readonly ValueModifiersManager ParryValueMultipliers = new ValueModifiersManager();

	public const int BaseMultiplier = 1;

	public const int SuperiorityMultiplier = 10;

	public const int BaseParryChance = 20;

	public FlagModifiersManager AutoParryModifier = new FlagModifiersManager();

	public FlagModifiersManager NeverParryModifier = new FlagModifiersManager();

	[CanBeNull]
	public AbilityData Ability { get; }

	public int Result { get; private set; }

	public int DeflectionResult { get; private set; }

	public int BaseChances { get; private set; }

	public int DefenderSkill { get; set; }

	public int AttackerWeaponSkill { get; set; }

	public int AttackerWeaponSkillOverride { get; set; }

	public bool IsAutoParry { get; private set; }

	public bool IsAutoHit { get; private set; }

	public bool IsRangedParry { get; private set; }

	public UseBallisticSkillToParry.BallisticSkillApplyPolicies BallisticSkillApplyPolicy { get; set; } = UseBallisticSkillToParry.BallisticSkillApplyPolicies.None;


	private bool UseBallisticSkill => Defender.Features.CanUseBallisticSkillToParry.Value;

	[CanBeNull]
	public MechanicEntity MaybeAttacker => base.MaybeTarget;

	[NotNull]
	public UnitEntity Defender => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public new NotImplementedException MaybeTarget
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public IEnumerable<Modifier> AllModifiersList
	{
		get
		{
			foreach (Modifier item in ParryValueModifiers.List)
			{
				yield return item;
			}
			foreach (Modifier item2 in AttackerWeaponSkillValueModifiers.List)
			{
				yield return item2;
			}
			foreach (Modifier item3 in DefenderCurrentAttackSkillValueModifiers.List)
			{
				yield return item3;
			}
		}
	}

	public int RawResult { get; private set; }

	public RuleCalculateParryChance([NotNull] UnitEntity defender, [CanBeNull] MechanicEntity attacker = null, [CanBeNull] AbilityData ability = null, int resultSuperiorityNumber = 0, bool isRangedParry = false, int attackerWeaponSkillOverride = 0)
		: base(defender, attacker)
	{
		Ability = ability;
		m_ResultSuperiorityNumber = resultSuperiorityNumber;
		IsRangedParry = isRangedParry;
		AttackerWeaponSkillOverride = attackerWeaponSkillOverride;
		base.HasNoTarget = attacker == null;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if ((IsRangedParry && !CanParryRanged(Defender)) || (!IsRangedParry && !CanParryMelee(Defender)))
		{
			RawResult = 0;
			Result = 0;
			DeflectionResult = 0;
			return;
		}
		DefenderSkill = Defender.Attributes.WarhammerWeaponSkill;
		if (UseBallisticSkill)
		{
			switch (BallisticSkillApplyPolicy)
			{
			case UseBallisticSkillToParry.BallisticSkillApplyPolicies.UseAlways:
				DefenderSkill = Defender.Attributes.WarhammerBallisticSkill;
				break;
			case UseBallisticSkillToParry.BallisticSkillApplyPolicies.UseIfGreaterThanOrEqual:
				DefenderSkill = (((int)Defender.Attributes.WarhammerBallisticSkill >= (int)Defender.Attributes.WarhammerWeaponSkill) ? Defender.Attributes.WarhammerBallisticSkill : Defender.Attributes.WarhammerWeaponSkill);
				break;
			}
		}
		int num = DefenderSkill + DefenderCurrentAttackSkillValueModifiers.Value;
		int num2 = 0;
		if (MaybeAttacker != null)
		{
			if (IsRangedParry)
			{
				AttackerWeaponSkill = AttackerWeaponSkillOverride;
			}
			else
			{
				AttackerWeaponSkill = MaybeAttacker.GetAttributeOptional(StatType.WarhammerWeaponSkill)?.ModifiedValue ?? 0;
			}
			RuleCalculateAttackPenalty ruleCalculateAttackPenalty = Rulebook.Trigger(new RuleCalculateAttackPenalty(MaybeAttacker, Ability));
			AttackerWeaponSkill = Math.Max(AttackerWeaponSkill - ruleCalculateAttackPenalty.ResultWeaponSkillPenalty, 0);
			num2 = AttackerWeaponSkill + AttackerWeaponSkillValueModifiers.Value;
		}
		else if (IsRangedParry)
		{
			AttackerWeaponSkill = AttackerWeaponSkillOverride;
			num2 = AttackerWeaponSkill;
		}
		if (Defender.HasMechanicFeature(MechanicsFeatureType.HiveOutnumber) && m_ResultSuperiorityNumber > 0)
		{
			m_ResultSuperiorityNumber--;
		}
		if (Defender.HasMechanicFeature(MechanicsFeatureType.IgnoreMeleeOutnumbering) && m_ResultSuperiorityNumber > 0)
		{
			m_ResultSuperiorityNumber = 0;
		}
		RawResult = 20 + num - (num2 + m_ResultSuperiorityNumber * 10) + ParryValueModifiers.Value;
		if (!ParryValueMultipliers.Empty)
		{
			RawResult *= ParryValueMultipliers.Value;
		}
		DeflectionResult = (IsRangedParry ? (Math.Clamp(RawResult / 2, 0, 100) + DefenderAttackRedirectionChanceModifiers.Value) : 0);
		Result = Math.Clamp(RawResult, 0, 95);
		SpecialOverrideWithFeatures();
	}

	private void SpecialOverrideWithFeatures()
	{
		if (MaybeAttacker != null && ((bool)MaybeAttacker.Features.AutoHit || NeverParryModifier.Value))
		{
			IsAutoHit = true;
			RawResult = 0;
			Result = 0;
			DeflectionResult = 0;
		}
		else if ((bool)Defender.Features.AutoParry || AutoParryModifier.Value)
		{
			IsAutoParry = true;
			RawResult = 100;
			Result = 100;
			DeflectionResult = (IsRangedParry ? 100 : 0);
		}
	}

	private bool CanParryMelee(BaseUnitEntity unit)
	{
		if (unit.CanAct)
		{
			if (unit.GetThreatHandMelee() == null)
			{
				if (unit.Features.CanUseBallisticSkillToParry.Value)
				{
					return unit.GetThreatHandRangedAnyHand() != null;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private bool CanParryRanged(BaseUnitEntity unit)
	{
		if (unit.CanAct && unit.HasMechanicFeature(MechanicsFeatureType.RangedParry))
		{
			return base.ConcreteInitiator.Facts.GetComponents((WarhammerDeflectionTarget c) => c.Caster == unit).Any();
		}
		return false;
	}
}
