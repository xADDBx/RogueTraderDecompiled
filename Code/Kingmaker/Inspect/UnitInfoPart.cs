using System;

namespace Kingmaker.Inspect;

[Flags]
public enum UnitInfoPart
{
	Base = 1,
	Defence = 2,
	Offence = 4,
	Abilities = 8,
	All = 0xF,
	None = 0
}
