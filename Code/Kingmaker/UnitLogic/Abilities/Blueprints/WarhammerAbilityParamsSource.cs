using System;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[Flags]
public enum WarhammerAbilityParamsSource
{
	None = 1,
	Weapon = 2,
	Item = 4,
	PsychicPower = 8,
	NavigatorPower = 0x10,
	SkillCheck = 0x20
}
