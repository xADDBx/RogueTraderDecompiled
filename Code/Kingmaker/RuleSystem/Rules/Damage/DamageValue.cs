using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.RuleSystem.Rules.Damage;

public struct DamageValue
{
	public readonly DamageData Source;

	public readonly int FinalValue;

	public readonly int RolledValue;

	public readonly int Reduction;

	public int ValueWithoutReduction => FinalValue + Reduction;

	public DamageValue(DamageData source, int finalValue, int rolledValue, int reduction)
	{
		Source = source;
		FinalValue = finalValue;
		RolledValue = rolledValue;
		Reduction = reduction;
	}
}
