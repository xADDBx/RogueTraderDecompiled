using System;

namespace Kingmaker.Enums;

[Flags]
public enum WeaponFamilyFlags
{
	None = 0,
	Laser = 2,
	Solid = 4,
	Bolt = 8,
	Melta = 0x10,
	Plasma = 0x20,
	Flame = 0x40,
	Exotic = 0x80,
	Chain = 0x100,
	Power = 0x200,
	Primitive = 0x400,
	Force = 0x800,
	Blade = 0x1000,
	ChainSaw = 0x2000
}
