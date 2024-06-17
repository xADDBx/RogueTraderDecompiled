using System;
using System.Text;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Newtonsoft.Json;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

[Serializable]
public class DamageDescription
{
	[JsonProperty]
	public DiceFormula Dice;

	[JsonProperty]
	public int Bonus;

	[JsonProperty]
	public DamageTypeDescription TypeDescription = new DamageTypeDescription();

	[SerializeReference]
	public IntEvaluator EvaluatedBonus;

	public bool CausedByCheckFail;

	private int m_BonusWithSource;

	private readonly ValueModifiersManager m_Modifiers = new ValueModifiersManager();

	public DamageData CreateDamage()
	{
		int num = Bonus - m_BonusWithSource;
		if (EvaluatedBonus != null)
		{
			num += EvaluatedBonus.GetValue();
		}
		DamageData damageData = TypeDescription.CreateDamage(Dice, num);
		damageData.CausedByCheckFail = CausedByCheckFail;
		return damageData;
	}

	public string GetReadableFormula()
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (Dice != DiceFormula.Zero)
		{
			stringBuilder.Append(Dice.ToString());
		}
		if (Bonus != 0)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" + ");
			}
			stringBuilder.Append(Bonus);
		}
		if (EvaluatedBonus != null)
		{
			if (stringBuilder.Length > 0)
			{
				stringBuilder.Append(" + ");
			}
			stringBuilder.Append(EvaluatedBonus.GetCaption());
		}
		return stringBuilder.ToString();
	}
}
