namespace Kingmaker.Utility.DotNetExtensions;

public static class MathExtensions
{
	public static int Pow2RoundUp(this int value)
	{
		value--;
		value |= value >> 1;
		value |= value >> 2;
		value |= value >> 4;
		value |= value >> 8;
		value |= value >> 16;
		return value + 1;
	}
}
