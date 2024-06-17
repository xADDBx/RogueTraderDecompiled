namespace Kingmaker.RuleSystem;

public static class DiceTypeExtension
{
	public static int Sides(this DiceType self)
	{
		return (int)self;
	}
}
