using System;

namespace Kingmaker.Enums;

[Flags]
public enum WeaponSubCategoryFlags
{
	Ranged = 2,
	Melee = 4,
	Finessable = 8,
	Thrown = 0x10,
	Natural = 0x20,
	Knives = 0x40,
	Monk = 0x80,
	TwoHanded = 0x100,
	Light = 0x200,
	Simple = 0x400,
	Martial = 0x800,
	Exotic = 0x1000,
	OneHandedPiercing = 0x2000,
	Disabled = 0x4000,
	OneHandedSlashing = 0x8000,
	Metal = 0x10000
}
