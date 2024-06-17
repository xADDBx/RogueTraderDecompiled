using System;

namespace Kingmaker.RuleSystem.Enum;

[Flags]
public enum AttackTypeFlag
{
	Melee = 1,
	Ranged = 2,
	All = 3
}
