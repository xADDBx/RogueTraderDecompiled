using System;

namespace Kingmaker.UnitLogic.Levelup.Selections;

[Flags]
public enum TalentGroup
{
	CharacteristicBonus = 1,
	SkillCheck = 2,
	SaveCheck = 4,
	Debuff = 8,
	Buff = 0x10,
	Psy = 0x20,
	Navigator = 0x40,
	Consumable = 0x80,
	EquipmentUsage = 0x100,
	Dodge = 0x200,
	Parry = 0x400,
	Armour = 0x800,
	Cover = 0x1000,
	TakingDamage = 0x2000,
	Crit = 0x4000,
	Damage = 0x8000,
	DoT = 0x10000,
	HeroicAct = 0x20000,
	Momentum = 0x40000,
	AP = 0x80000,
	MP = 0x100000,
	HealWound = 0x200000,
	SpaceCombat = 0x400000,
	Homeworld = 0x800000,
	Occupation = 0x1000000
}
