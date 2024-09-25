using System;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.Mechanics.Damage;

public static class DamageExtension
{
	private const DamageTypeMask PhysicalMask = DamageTypeMask.Impact | DamageTypeMask.Rending | DamageTypeMask.Piercing | DamageTypeMask.Power;

	private const DamageTypeMask ForceMask = DamageTypeMask.Fire | DamageTypeMask.Shock | DamageTypeMask.Toxic | DamageTypeMask.Energy;

	private const DamageTypeMask PermeatingMask = DamageTypeMask.Warp | DamageTypeMask.Neural | DamageTypeMask.Surge | DamageTypeMask.Direct;

	private static readonly DamageCategoryInfo[] Categories;

	private static readonly DamageTypeInfo[] Types;

	static DamageExtension()
	{
		Categories = new DamageCategoryInfo[3]
		{
			new DamageCategoryInfo(DamageCategory.Physical, DamageTypeMask.Impact | DamageTypeMask.Rending | DamageTypeMask.Piercing | DamageTypeMask.Power, ignoreArmor: false),
			new DamageCategoryInfo(DamageCategory.Force, DamageTypeMask.Fire | DamageTypeMask.Shock | DamageTypeMask.Toxic | DamageTypeMask.Energy, ignoreArmor: false),
			new DamageCategoryInfo(DamageCategory.Permeating, DamageTypeMask.Warp | DamageTypeMask.Neural | DamageTypeMask.Surge | DamageTypeMask.Direct, ignoreArmor: true)
		};
		Types = new DamageTypeInfo[13]
		{
			new DamageTypeInfo(DamageType.Impact, DamageCategory.Physical),
			new DamageTypeInfo(DamageType.Rending, DamageCategory.Physical),
			new DamageTypeInfo(DamageType.Piercing, DamageCategory.Physical),
			new DamageTypeInfo(DamageType.Power, DamageCategory.Physical),
			new DamageTypeInfo(DamageType.Fire, DamageCategory.Force),
			new DamageTypeInfo(DamageType.Shock, DamageCategory.Force),
			new DamageTypeInfo(DamageType.Toxic, DamageCategory.Force),
			new DamageTypeInfo(DamageType.Energy, DamageCategory.Force),
			new DamageTypeInfo(DamageType.Warp, DamageCategory.Permeating),
			new DamageTypeInfo(DamageType.Neural, DamageCategory.Permeating),
			new DamageTypeInfo(DamageType.Surge, DamageCategory.Permeating),
			new DamageTypeInfo(DamageType.Direct, DamageCategory.Permeating),
			new DamageTypeInfo(DamageType.Ram, DamageCategory.Physical)
		};
	}

	public static DamageData CreateDamage(this DamageType type, int value)
	{
		return new DamageData(type, value);
	}

	public static DamageData CreateDamage(this DamageType type, int min, int max)
	{
		return new DamageData(type, min, max);
	}

	public static DamageData CreateDamage(this DamageType type, DiceFormula dice, int bonus = 0)
	{
		return new DamageData(type, dice.MinValue(bonus), dice.MaxValue(bonus));
	}

	public static bool Contains(this DamageTypeMask mask, DamageType damageType)
	{
		int num = 1 << (int)damageType;
		return ((uint)mask & (uint)num) == (uint)num;
	}

	public static DamageCategoryInfo GetInfo(this DamageCategory category)
	{
		return Categories[(int)category] ?? throw new ArgumentOutOfRangeException("category", category, "category info is missing");
	}

	public static DamageTypeInfo GetInfo(this DamageType type)
	{
		return Types[(int)type] ?? throw new ArgumentOutOfRangeException("type", type, "damage type info is missing");
	}
}
