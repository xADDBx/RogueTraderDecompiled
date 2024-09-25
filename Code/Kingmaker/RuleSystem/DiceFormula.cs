using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.RuleSystem;

[Serializable]
[JsonObject(IsReference = false)]
public struct DiceFormula
{
	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	private int m_Rolls;

	[SerializeField]
	[HideInInspector]
	[JsonProperty]
	private DiceType m_Dice;

	public static readonly DiceFormula Zero = new DiceFormula(0, DiceType.Zero);

	public static readonly DiceFormula One = new DiceFormula(1, DiceType.One);

	public int Rolls
	{
		get
		{
			if (m_Dice != 0)
			{
				return m_Rolls;
			}
			return 0;
		}
	}

	public DiceType Dice => m_Dice;

	public DiceFormula(int rollsCount, DiceType diceType)
	{
		m_Rolls = Math.Max(0, rollsCount);
		m_Dice = diceType;
	}

	public override string ToString()
	{
		if (!(this == Zero) && !(this == One))
		{
			return $"{m_Rolls}d{(int)m_Dice}";
		}
		return Rolls.ToString();
	}

	public string ToNumString(int bonus, bool halfDamage = false)
	{
		if (!(this == Zero) && !(this == One))
		{
			return $"{MinValue(bonus, halfDamage)}-{MaxValue(bonus, halfDamage)}";
		}
		return Rolls.ToString();
	}

	public int MinValue(int bonus, bool halfDamage = false)
	{
		int num = Math.Max(m_Rolls + bonus, 0);
		if (!halfDamage)
		{
			return num;
		}
		return num / 2;
	}

	public int MaxValue(int bonus, bool halfDamage = false)
	{
		int num = Math.Max((int)Dice * Rolls + bonus, 0);
		if (!halfDamage)
		{
			return num;
		}
		return num / 2;
	}

	public bool Equals(DiceFormula other)
	{
		if (m_Rolls == other.m_Rolls)
		{
			return m_Dice == other.m_Dice;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj == null)
		{
			return false;
		}
		if (obj is DiceFormula)
		{
			return Equals((DiceFormula)obj);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (Rolls * 397) ^ (int)Dice;
	}

	public static bool operator ==(DiceFormula f1, DiceFormula f2)
	{
		return f1.Equals(f2);
	}

	public static bool operator !=(DiceFormula f1, DiceFormula f2)
	{
		return !(f1 == f2);
	}
}
