using System;

namespace Kingmaker.Enums;

[Flags]
public enum WeaponCategoryFlags
{
	Unarmed = 1,
	Melee = 2,
	Thrown = 4,
	Pistol = 8,
	Basic = 0x10,
	Heavy = 0x20
}
