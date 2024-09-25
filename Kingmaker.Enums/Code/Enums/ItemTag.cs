using System;

namespace Code.Enums;

[Flags]
public enum ItemTag
{
	None = 0,
	SkillCheckConsumable = 1,
	Grenade = 2,
	Medikit = 4,
	Drug = 8,
	Pet = 0x10
}
