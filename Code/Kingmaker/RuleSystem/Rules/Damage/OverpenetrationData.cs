using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

public struct OverpenetrationData
{
	public float? DamageRoll;

	public int MinBaseValue;

	public int MaxBaseValue;

	public int OverpenetrationPercent;

	public OverpenetrationData(DamageData overpenetrationDamage)
	{
		DamageRoll = overpenetrationDamage.Roll;
		MinBaseValue = overpenetrationDamage.MinValueBase;
		MaxBaseValue = overpenetrationDamage.MaxValueBase;
		OverpenetrationPercent = overpenetrationDamage.OverpenetrationFactorPercents;
	}
}
