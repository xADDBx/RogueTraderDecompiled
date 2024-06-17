using System;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

[Flags]
public enum DamageTypeMask
{
	None = 0,
	Impact = 1,
	Rending = 2,
	Piercing = 4,
	Power = 8,
	Fire = 0x10,
	Shock = 0x20,
	Toxic = 0x40,
	Energy = 0x80,
	Warp = 0x100,
	Neural = 0x200,
	Surge = 0x400,
	Direct = 0x800
}
