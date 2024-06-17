namespace Kingmaker.UnitLogic.Abilities;

public class HealPredictionData
{
	public int Value;

	public int Bonus;

	public static HealPredictionData operator +(HealPredictionData lhs, HealPredictionData rhs)
	{
		return new HealPredictionData
		{
			Value = lhs.Value + rhs.Value,
			Bonus = lhs.Bonus + rhs.Bonus
		};
	}
}
