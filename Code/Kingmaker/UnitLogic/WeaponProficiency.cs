using System;
using Kingmaker.Enums;

namespace Kingmaker.UnitLogic;

[Serializable]
public struct WeaponProficiency
{
	public WeaponCategory Category;

	public WeaponFamily Family;

	public bool Universal;

	public WeaponProficiency(WeaponCategory category, WeaponFamily family)
	{
		Category = category;
		Family = family;
		Universal = false;
	}

	public WeaponProficiency(WeaponCategory category)
	{
		Category = category;
		Family = WeaponFamily.Primitive;
		Universal = true;
	}
}
