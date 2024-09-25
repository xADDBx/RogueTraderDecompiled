namespace Kingmaker.Utility;

public static class FeetExtension
{
	public static Feet Feet(this int self)
	{
		return new Feet(self);
	}

	public static Feet Feet(this float self)
	{
		return new Feet(self);
	}
}
