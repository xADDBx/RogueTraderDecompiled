using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.RuleSystem.Rules;

public class RuleRollDice : RulebookEvent
{
	public readonly DiceFormula DiceFormula;

	private int m_Result;

	private int m_RerollAmount;

	private int? m_PreRolledResult;

	protected int? ResultOverride;

	private int m_ResultModifier;

	public bool ReplaceOneWithMax { get; set; }

	public List<int> RollHistory { get; private set; }

	[CanBeNull]
	public List<RerollData> Rerolls { get; private set; }

	public bool ReplacedOne
	{
		get
		{
			if (ReplaceOneWithMax)
			{
				return m_Result == 1;
			}
			return false;
		}
	}

	public int Result => m_ResultModifier + (ReplacedOne ? DiceFormula.MaxValue(0) : (ResultOverride ?? m_Result));

	public RuleRollDice(IMechanicEntity initiator, DiceFormula diceFormula)
		: base(initiator)
	{
		DiceFormula = diceFormula;
	}

	public RuleRollDice(MechanicEntity initiator, DiceFormula diceFormula, int resultOverride)
		: base(initiator)
	{
		DiceFormula = diceFormula;
		ResultOverride = resultOverride;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		if (Result <= 0)
		{
			Roll();
		}
	}

	private void Roll()
	{
		m_Result = m_PreRolledResult ?? Dice.D(DiceFormula);
		RollHistory = new List<int> { m_Result };
		for (int num = Math.Abs(m_RerollAmount); num > 0; num--)
		{
			Reroll();
		}
	}

	private void Reroll()
	{
		int num = Dice.D(DiceFormula);
		RollHistory.Add(num);
		int val = ((num == 1 && ReplaceOneWithMax) ? DiceFormula.MaxValue(0) : num);
		m_Result = ((m_RerollAmount > 0) ? Math.Min(m_Result, val) : Math.Max(m_Result, val));
	}

	public void Reroll(MechanicEntityFact rerollSource, bool takeBest)
	{
		AddReroll(1, takeBest, rerollSource);
		Reroll();
	}

	public int PreRollDice()
	{
		if (Result > 0 || m_PreRolledResult.HasValue)
		{
			if (Result <= 0)
			{
				return m_PreRolledResult.GetValueOrDefault();
			}
			return Result;
		}
		m_PreRolledResult = Dice.D(DiceFormula);
		return m_PreRolledResult.Value;
	}

	public void AddReroll(int amount, bool takeBest, MechanicEntityFact rerollSource)
	{
		amount = (takeBest ? amount : (-amount));
		m_RerollAmount += amount;
		Rerolls = Rerolls ?? new List<RerollData>();
		Rerolls.Add(new RerollData(amount, rerollSource));
	}

	public virtual void Override(int roll)
	{
		ResultOverride = roll;
	}

	public void AddModifier(int modifier)
	{
		m_ResultModifier += modifier;
	}

	public static implicit operator int(RuleRollDice ruleRollDice)
	{
		return ruleRollDice?.Result ?? 0;
	}

	public override string ToString()
	{
		if (RollHistory == null || RollHistory.Count == 1)
		{
			return m_Result.ToString();
		}
		StringBuilder stringBuilder = new StringBuilder();
		bool flag = true;
		bool flag2 = false;
		foreach (int item in RollHistory)
		{
			if (!flag)
			{
				stringBuilder.Append(", ");
			}
			flag = false;
			if (item == m_Result && !flag2)
			{
				stringBuilder.Append("<b><u>").Append(item).Append("</u></b>");
				flag2 = true;
			}
			else
			{
				stringBuilder.Append(item);
			}
		}
		if (Rerolls.Count > 0)
		{
			stringBuilder.Append(" [").Append(Rerolls[Rerolls.Count - 1].Source.Name).Append("]");
		}
		return stringBuilder.ToString();
	}
}
