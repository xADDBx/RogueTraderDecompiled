namespace Kingmaker.View.Animation;

public static class WeaponAnimationStyleExtensions
{
	public static bool IsTwoHanded(this WeaponAnimationStyle style)
	{
		switch (style)
		{
		case WeaponAnimationStyle.AxeTwoHanded:
		case WeaponAnimationStyle.Assault:
		case WeaponAnimationStyle.BrutalTwoHanded:
		case WeaponAnimationStyle.HeavyOnHip:
		case WeaponAnimationStyle.HeavyOnShoulder:
		case WeaponAnimationStyle.Rifle:
		case WeaponAnimationStyle.Staff:
		case WeaponAnimationStyle.EldarRifle:
		case WeaponAnimationStyle.EldarAssault:
		case WeaponAnimationStyle.EldarHeavyOnHip:
		case WeaponAnimationStyle.EldarHeavyOnShoulder:
		case WeaponAnimationStyle.TwoHandedHammer:
			return true;
		default:
			return false;
		}
	}
}
