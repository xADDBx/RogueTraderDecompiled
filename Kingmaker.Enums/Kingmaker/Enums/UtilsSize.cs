namespace Kingmaker.Enums;

public static class UtilsSize
{
	public static bool IsBigAndEven(this Size size)
	{
		return size == Size.Large || size == Size.Cruiser_2x4;
	}

	public static bool IsBigAndEvenUnit(this Size size)
	{
		return size == Size.Large;
	}

	public static bool Is1x1(this Size size)
	{
		if (size != 0 && size != Size.Diminutive && size != Size.Tiny && size != Size.Small && size != Size.Medium)
		{
			return size == Size.Raider_1x1;
		}
		return true;
	}
}
