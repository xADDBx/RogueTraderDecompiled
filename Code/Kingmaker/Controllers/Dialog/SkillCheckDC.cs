using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class SkillCheckDC
{
	[CanBeNull]
	public BaseUnitEntity ActingUnit;

	public StatType StatType;

	public int ConditionDC;

	public int ValueDC;

	public bool IsBestParameter;

	public bool? FakePassed;

	public int? FakeRoll;

	public bool IsSatisf => ActingUnit?.Stats.GetStat(StatType).ModifiedValue >= ConditionDC;

	public SkillCheckDC()
	{
	}

	public SkillCheckDC(BaseUnitEntity unit, StatType statType, int conditionDC, int valueDC, bool isBest, bool? isPassed)
	{
		ActingUnit = unit;
		StatType = statType;
		ConditionDC = conditionDC;
		IsBestParameter = isBest;
		ValueDC = valueDC;
		FakePassed = isPassed;
		FakeRoll = (isPassed.HasValue ? GetFakeRoll(ConditionDC + ValueDC, isPassed.Value) : null);
	}

	private int? GetFakeRoll(int chance, bool passed)
	{
		int num = ActingUnit?.GetHashCode() ?? 0;
		num *= (int)StatType;
		num ^= 0xFFFFFD;
		if (num < 0)
		{
			num *= -1;
		}
		chance = Mathf.Clamp(chance, 0, 100);
		int num2 = ((!passed) ? (chance + 1) : 0);
		int num3 = (passed ? chance : 101);
		return num2 + num % Mathf.Max(1, num3 - num2);
	}
}
