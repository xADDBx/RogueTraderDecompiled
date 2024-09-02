using System;

namespace Kingmaker.UnitLogic.Abilities.Blueprints;

[Flags]
public enum AreaEffectRestrictions
{
	None = 1,
	CanOnlyUseWeaponAbilities = 2,
	CannotUseWeaponAbilities = 4,
	CannotUsePsychicPowers = 8
}
