using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features.Advancements;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollChance : RuleRollD100
{
	private int? m_RerollSuccessChance;

	private int? m_RerollFailChance;

	private int m_RerollCount;

	private string m_RerollSourceFactName = "";

	public int Chance { get; }

	public RollChanceType Type { get; }

	public RollType? RollTypeValue { get; }

	public BlueprintSkillAdvancement.SkillType? SkillType { get; private set; }

	public BlueprintAttributeAdvancement.AttributeType? AttributeType { get; private set; }

	public bool Success { get; private set; }

	public IMechanicEntity AttackInitiator { get; }

	public bool IsResultOverriden => ResultOverride.HasValue;

	public int? RerollChance
	{
		get
		{
			if (!Success)
			{
				return m_RerollFailChance;
			}
			return m_RerollSuccessChance;
		}
	}

	public int? AnyRerollChance => m_RerollSuccessChance ?? m_RerollFailChance;

	public string RerollSourceFactName => m_RerollSourceFactName;

	public RuleRollChance([NotNull] IMechanicEntity initiator, int chance, RollType? rollType = null, RollChanceType type = RollChanceType.Untyped, int? resultOverride = null, IMechanicEntity attackInitiator = null)
		: base(initiator)
	{
		Chance = chance;
		Type = type;
		RollTypeValue = rollType;
		ResultOverride = resultOverride;
		AttackInitiator = attackInitiator;
	}

	public override void Override(int roll)
	{
		base.Override(roll);
		Success = base.Result <= Chance;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		base.OnTrigger(context);
		Success = base.Result <= Chance;
		int? rerollChance = RerollChance;
		if (rerollChance.HasValue)
		{
			bool success = Success;
			int result = base.Result;
			int num = 0;
			while (success == Success && m_RerollCount > num)
			{
				Reroll((base.Initiator as MechanicEntity)?.MainFact, !success);
				Success = base.Result <= rerollChance.Value;
				num++;
			}
			PFLog.Default.Log("Reroll {0} success={1}->{2}, chance={3}->{4}, result={5}->{6}, initiator : {7} {8}, rerollCount={9}/{10}", RollTypeValue, success, Success, Chance, rerollChance.Value, result, base.Result, (base.Initiator as MechanicEntity)?.UniqueId, (base.Initiator as BaseUnitEntity)?.CharacterName, num, m_RerollCount);
		}
	}

	public void AddRerollSuccess(int chanceValue, int rerollCount, MechanicEntityFact sourceFact)
	{
		m_RerollSourceFactName = sourceFact.Name;
		m_RerollSuccessChance = (m_RerollSuccessChance.HasValue ? Math.Max(chanceValue, m_RerollSuccessChance.Value) : chanceValue);
		m_RerollCount = rerollCount;
	}

	public void AddRerollFail(int chanceValue, int maxRerollCount, MechanicEntityFact sourceFact)
	{
		m_RerollSourceFactName = sourceFact.Name;
		m_RerollFailChance = (m_RerollFailChance.HasValue ? Math.Max(chanceValue, m_RerollFailChance.Value) : chanceValue);
		m_RerollCount = maxRerollCount;
	}

	public void SetSuccess(bool value)
	{
		Success = value;
	}

	public static RuleRollChance FromInt(IMechanicEntity initiator, int chance, int roll, RollType? rollType, RollChanceType type = RollChanceType.Untyped, IMechanicEntity attackInitiator = null, StatType? statType = null)
	{
		RuleRollChance ruleRollChance = new RuleRollChance(initiator, chance, rollType, type, roll, attackInitiator);
		if (statType.HasValue)
		{
			RollType? rollTypeByStat = GetRollTypeByStat(statType.Value);
			if (rollTypeByStat == RollType.Skill)
			{
				ruleRollChance.SkillType = BlueprintSkillAdvancement.GetSkillTypeFromStatType(statType.Value);
			}
			if (rollTypeByStat == RollType.Attribute)
			{
				ruleRollChance.AttributeType = BlueprintAttributeAdvancement.GetAttributeTypeFromStatType(statType.Value);
			}
		}
		Rulebook.Trigger(ruleRollChance);
		return ruleRollChance;
	}

	public static RuleRollChance FromInt(IMechanicEntity initiator, int chance, int roll, StatType statType, RollChanceType type = RollChanceType.Untyped, IMechanicEntity attackInitiator = null)
	{
		return FromInt(initiator, chance, roll, GetRollTypeByStat(statType), type, attackInitiator, statType);
	}

	public static RuleRollChance Roll(IMechanicEntity initiator, int chance, StatType statType, RollChanceType type = RollChanceType.Untyped, IMechanicEntity attackInitiator = null)
	{
		return Roll(initiator, chance, GetRollTypeByStat(statType), type, attackInitiator, statType);
	}

	public static RuleRollChance Roll(IMechanicEntity initiator, int chance, RollType? rollType = null, RollChanceType type = RollChanceType.Untyped, IMechanicEntity attackInitiator = null, StatType? statType = null)
	{
		RuleRollChance ruleRollChance = new RuleRollChance(initiator, chance, rollType, type, null, attackInitiator);
		if (statType.HasValue)
		{
			RollType? rollTypeByStat = GetRollTypeByStat(statType.Value);
			if (rollTypeByStat == RollType.Skill)
			{
				ruleRollChance.SkillType = BlueprintSkillAdvancement.GetSkillTypeFromStatType(statType.Value);
			}
			if (rollTypeByStat == RollType.Attribute)
			{
				ruleRollChance.AttributeType = BlueprintAttributeAdvancement.GetAttributeTypeFromStatType(statType.Value);
			}
		}
		Rulebook.Trigger(ruleRollChance);
		return ruleRollChance;
	}

	public static RollType? GetRollTypeByStat(StatType statType)
	{
		if (statType.IsSkill())
		{
			return RollType.Skill;
		}
		if (statType.IsAttribute())
		{
			return RollType.Attribute;
		}
		return null;
	}
}
