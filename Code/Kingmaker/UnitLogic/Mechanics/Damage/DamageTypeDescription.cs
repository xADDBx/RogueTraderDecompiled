using System;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

[Serializable]
public class DamageTypeDescription
{
	public DamageType Type;

	public DamageDescription GetDamageDescriptor(DiceFormula dice, int bonus)
	{
		return new DamageDescription
		{
			TypeDescription = Copy(),
			Dice = dice,
			Bonus = bonus
		};
	}

	public DamageData CreateDamage(DiceFormula dice, int bonus)
	{
		return Type.CreateDamage(dice, bonus);
	}

	public DamageData CreateDamage(int min, int max)
	{
		return Type.CreateDamage(min, max);
	}

	public DamageTypeDescription Copy()
	{
		return new DamageTypeDescription
		{
			Type = Type
		};
	}

	public override string ToString()
	{
		return $"{Type} damage";
	}
}
