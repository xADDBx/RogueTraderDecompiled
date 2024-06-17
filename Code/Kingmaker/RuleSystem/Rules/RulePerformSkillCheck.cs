using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.Settings;
using Kingmaker.Utility.Random;

namespace Kingmaker.RuleSystem.Rules;

public class RulePerformSkillCheck : RulebookEvent
{
	[Flags]
	public enum VoicingType
	{
		None = 0,
		Success = 1,
		Failure = 2,
		All = 3
	}

	public readonly ValueModifiersManager DifficultyModifiers = new ValueModifiersManager();

	private bool m_ValueCalculated;

	public BaseUnitEntity SkinningTarget { get; }

	public int BaseDifficulty { get; }

	public int StatValue { get; private set; }

	public bool Silent { get; set; }

	public VoicingType Voice { get; set; }

	public bool IgnoreDifficultyBonusToDC { get; set; }

	public bool ShowAnyway { get; set; }

	public StatType StatType { get; }

	public bool? EnsureSuccess { get; set; }

	public RuleRollChance ResultChanceRule { get; private set; }

	public bool ResultIsSuccess { get; private set; }

	public bool ResultIsCriticalFail { get; private set; }

	public int ResultDegreeOfSuccess { get; private set; }

	public int Difficulty => BaseDifficulty + DifficultyModifiers.Value;

	public int EffectiveSkill => StatValue + Difficulty;

	public int RollResult => ResultChanceRule;

	public int Chance => CalculateChances(EffectiveSkill, Difficulty);

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, RulePerformSkillCheck sourceCheck)
		: this(unit, sourceCheck.StatType, sourceCheck.BaseDifficulty)
	{
		Silent = sourceCheck.Silent;
		Voice = sourceCheck.Voice;
		SkinningTarget = sourceCheck.SkinningTarget;
		IgnoreDifficultyBonusToDC = sourceCheck.IgnoreDifficultyBonusToDC;
		ShowAnyway = sourceCheck.ShowAnyway;
		EnsureSuccess = sourceCheck.EnsureSuccess;
		DifficultyModifiers.CopyFrom(sourceCheck.DifficultyModifiers, (Modifier i) => i.Descriptor != ModifierDescriptor.Difficulty);
	}

	public RulePerformSkillCheck([NotNull] MechanicEntity unit, StatType statType, int difficulty)
		: base(unit)
	{
		StatType = statType;
		BaseDifficulty = difficulty;
		UpdateValues();
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (!UpdateValues())
		{
			PFLog.Default.Error($"Invalid stat {StatType}");
			return;
		}
		if (!EnsureSuccess.HasValue)
		{
			ResultChanceRule = RollChanceRule();
		}
		else
		{
			int minInclusive = 1;
			int maxExclusive = 100;
			int effectiveSkill = EffectiveSkill;
			if (EnsureSuccess.Value)
			{
				maxExclusive = Math.Max(1, Math.Min(effectiveSkill, 100));
			}
			else
			{
				minInclusive = Math.Min(100, Math.Max(effectiveSkill, 1));
			}
			ResultChanceRule = RuleRollChance.FromInt(base.Initiator, GetSuccessChance(), PFStatefulRandom.RuleSystem.Range(minInclusive, maxExclusive), StatType);
		}
		ResultIsSuccess = ResultChanceRule.Success;
		ResultIsCriticalFail = !ResultIsSuccess && (int)ResultChanceRule - EffectiveSkill > 30;
		int num = EffectiveSkill - (int)ResultChanceRule;
		ResultDegreeOfSuccess = num / 10 + ((num >= 0) ? 1 : (-1));
	}

	public void Calculate(bool doCheck = true)
	{
		StatValue = ((base.Initiator as MechanicEntity)?.GetStatOptional(StatType))?.ModifiedValue ?? 0;
		if (doCheck)
		{
			ResultChanceRule = RollChanceRule();
		}
	}

	public int GetSuccessChance(int successBonus = 0)
	{
		if (StatType != 0)
		{
			return EffectiveSkill + successBonus;
		}
		return 100;
	}

	public static int CalculateChances(int bonus, int dc)
	{
		return Math.Max(Math.Min(bonus, 100), 0);
	}

	protected virtual RuleRollChance RollChanceRule()
	{
		return RuleRollChance.Roll(base.Initiator, GetSuccessChance(), StatType);
	}

	private bool UpdateValues()
	{
		if (m_ValueCalculated)
		{
			return true;
		}
		ModifiableValue modifiableValue = (base.Initiator as MechanicEntity)?.GetStatOptional(StatType);
		if (modifiableValue != null)
		{
			StatValue = modifiableValue.ModifiedValue;
			if (!IgnoreDifficultyBonusToDC)
			{
				DifficultyModifiers.Add(SettingsRoot.Difficulty.SkillCheckModifier, this, ModifierDescriptor.Difficulty);
			}
			m_ValueCalculated = true;
			return true;
		}
		return false;
	}
}
