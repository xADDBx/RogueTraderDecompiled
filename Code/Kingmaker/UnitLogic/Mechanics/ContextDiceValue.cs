using System;
using Kingmaker.RuleSystem;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextDiceValue
{
	[HideInInspector]
	public DiceType DiceType;

	[HideInInspector]
	public ContextValue DiceCountValue;

	[HideInInspector]
	public ContextValue BonusValue;

	public bool IsVariable => DiceType > DiceType.One;

	public int Calculate(MechanicsContext context)
	{
		return ContextValueHelper.CalculateDiceValue(DiceType, DiceCountValue, BonusValue, context);
	}

	public override string ToString()
	{
		bool num = DiceCountValue.IsValueSimple && DiceCountValue.Value == 0;
		bool flag = BonusValue.IsValueSimple && BonusValue.Value == 0;
		if (!num)
		{
			if (!flag)
			{
				return $"{DiceCountValue}{DiceType}+{BonusValue}";
			}
			return $"{DiceCountValue}{DiceType}";
		}
		if (!flag)
		{
			return BonusValue.ToString();
		}
		return "0";
	}
}
