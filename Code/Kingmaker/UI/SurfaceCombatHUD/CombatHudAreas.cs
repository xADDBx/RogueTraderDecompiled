using System;

namespace Kingmaker.UI.SurfaceCombatHUD;

[Flags]
public enum CombatHudAreas : ushort
{
	Walkable = 1,
	Movement = 2,
	ActiveUnit = 4,
	AttackOfOpportunity = 8,
	AbilityMinRange = 0x10,
	AbilityMaxRange = 0x20,
	AbilityEffectiveRange = 0x40,
	AbilityPrimary = 0x80,
	AbilitySecondary = 0x100,
	StratagemAlly = 0x200,
	StratagemAllyIntersection = 0x400,
	StratagemHostile = 0x800,
	StratagemHostileIntersection = 0x1000,
	SpaceMovement1 = 0x2000,
	SpaceMovement2 = 0x4000,
	SpaceMovement3 = 0x8000
}
