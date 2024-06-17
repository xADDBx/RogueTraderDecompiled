using System;
using Kingmaker.RuleSystem;
using Kingmaker.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.UnitLogic.Mechanics;

[Serializable]
public class ContextDurationValue
{
	public DurationRate Rate;

	public DiceType DiceType;

	public ContextValue DiceCountValue;

	public ContextValue BonusValue;

	[SerializeField]
	[FormerlySerializedAs("IsExtendable")]
	private bool m_IsExtendable = true;

	public bool IsExtendable
	{
		get
		{
			if (!m_IsExtendable)
			{
				return false;
			}
			bool num = DiceType > DiceType.Zero && !DiceCountValue.IsZero;
			bool flag = !BonusValue.IsZero;
			return num || flag;
		}
	}

	public bool IsVariable => DiceType > DiceType.One;

	public Rounds Calculate(MechanicsContext context)
	{
		int num = ContextValueHelper.CalculateDiceValue(DiceType, DiceCountValue, BonusValue, context);
		return Rate.ToRounds() * num;
	}

	public override string ToString()
	{
		bool num = DiceCountValue.IsValueSimple && DiceCountValue.Value == 0;
		bool flag = BonusValue.IsValueSimple && BonusValue.Value == 0;
		string arg = ((!num) ? (flag ? $"{DiceCountValue}{DiceType}" : $"{DiceCountValue}{DiceType}+{BonusValue}") : (flag ? "0" : BonusValue.ToString()));
		return $"{arg} {Rate}";
	}
}
