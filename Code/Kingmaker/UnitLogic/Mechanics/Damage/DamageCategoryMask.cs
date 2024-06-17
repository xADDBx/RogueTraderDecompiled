using System;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

[Flags]
public enum DamageCategoryMask
{
	None = 0,
	Physical = 1,
	Force = 2,
	Permeating = 4
}
