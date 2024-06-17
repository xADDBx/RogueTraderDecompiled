using System;

namespace Kingmaker.RuleSystem.Rules;

[Flags]
public enum RollType
{
	Dodge = 1,
	Parry = 2,
	Attack = 4,
	Skill = 8,
	Attribute = 0x10
}
