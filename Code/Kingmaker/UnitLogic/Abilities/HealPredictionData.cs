namespace Kingmaker.UnitLogic.Abilities;

public class HealPredictionData
{
	public int Bonus;

	public int MinValue;

	public int MaxValue;

	public static HealPredictionData operator +(HealPredictionData lhs, HealPredictionData rhs)
	{
		return new HealPredictionData
		{
			Bonus = lhs.Bonus + rhs.Bonus,
			MinValue = lhs.MinValue + rhs.MinValue,
			MaxValue = lhs.MaxValue + rhs.MaxValue
		};
	}
}
