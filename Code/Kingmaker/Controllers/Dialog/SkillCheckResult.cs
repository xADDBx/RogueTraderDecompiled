using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;

namespace Kingmaker.Controllers.Dialog;

public class SkillCheckResult
{
	public BaseUnitEntity ActingUnit;

	public StatType StatType;

	public int DC;

	public bool Passed;

	public int RollResult;

	public int D100;

	public int StatValue;

	public int TotalSkill;

	public SkillCheckResult(RulePerformPartySkillCheck e)
	{
		ActingUnit = e.Roller;
		StatType = e.StatType;
		DC = e.Difficulty;
		Passed = e.Success;
		RollResult = e.RollResult;
		D100 = e.D100;
		StatValue = e.StatValue;
		TotalSkill = e.TotalSkill;
	}

	public SkillCheckResult(RulePerformSkillCheck e, BaseUnitEntity unit)
	{
		ActingUnit = unit;
		StatType = e.StatType;
		DC = e.Difficulty;
		Passed = e.ResultIsSuccess;
		RollResult = e.RollResult;
		D100 = e.ResultChanceRule;
		StatValue = e.StatValue;
		TotalSkill = e.EffectiveSkill;
	}

	public SkillCheckResult(BaseUnitEntity actingUnit, StatType statType, int dc)
	{
		ActingUnit = actingUnit;
		StatType = statType;
		DC = dc;
		Passed = false;
	}
}
