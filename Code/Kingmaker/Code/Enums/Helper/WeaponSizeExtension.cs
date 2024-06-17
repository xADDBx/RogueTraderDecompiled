using System.Collections.Generic;
using Kingmaker.Enums;

namespace Kingmaker.Code.Enums.Helper;

public static class WeaponSizeExtension
{
	public class SizeModifiers
	{
		public readonly int AttackAndAC;

		public readonly int CMDAndCMD;

		public readonly int Stealth;

		public readonly int Reach;

		public SizeModifiers(int attackAndAC, int cmdAndCMD, int stealth, int reach)
		{
			AttackAndAC = attackAndAC;
			CMDAndCMD = cmdAndCMD;
			Stealth = stealth;
			Reach = reach;
		}
	}

	private static readonly Dictionary<Size, SizeModifiers> Modifiers = new Dictionary<Size, SizeModifiers>
	{
		{
			Size.Fine,
			new SizeModifiers(8, -8, 16, 0)
		},
		{
			Size.Diminutive,
			new SizeModifiers(4, -4, 12, 0)
		},
		{
			Size.Tiny,
			new SizeModifiers(2, -4, 8, 0)
		},
		{
			Size.Small,
			new SizeModifiers(1, -2, 4, 5)
		},
		{
			Size.Medium,
			new SizeModifiers(0, 0, 0, 5)
		},
		{
			Size.Large,
			new SizeModifiers(-1, 2, -4, 9)
		},
		{
			Size.Huge,
			new SizeModifiers(-2, 4, -8, 12)
		},
		{
			Size.Gargantuan,
			new SizeModifiers(-4, 8, -12, 15)
		},
		{
			Size.Colossal,
			new SizeModifiers(-8, 16, -16, 18)
		}
	};

	public static Size Shift(this Size size, int shift)
	{
		Size size2 = size + shift;
		if (size2 >= Size.Tiny)
		{
			if (size2 <= Size.Colossal)
			{
				return size2;
			}
			return Size.Colossal;
		}
		return Size.Tiny;
	}

	public static SizeModifiers GetModifiers(this Size size)
	{
		if (!Modifiers.TryGetValue(size, out var value))
		{
			PFLog.Default.Error("Has no modifiers for size '{0}'", size);
		}
		return value;
	}
}
