using System;

namespace Kingmaker.Globalmap.CombatRandomEncounters;

[Flags]
public enum UnitRole
{
	None = 0,
	Sniper = 2,
	Melee = 4,
	FlameThrower = 8,
	Range = 0x10,
	Elite = 0x20,
	Specialist = 0x40
}
