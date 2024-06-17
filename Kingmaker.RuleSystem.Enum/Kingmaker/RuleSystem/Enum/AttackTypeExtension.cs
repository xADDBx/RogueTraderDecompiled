namespace Kingmaker.RuleSystem.Enum;

public static class AttackTypeExtension
{
	public static bool IsMelee(this AttackType attackType)
	{
		return attackType == AttackType.Melee;
	}

	public static bool IsRanged(this AttackType attackType)
	{
		return attackType == AttackType.Ranged;
	}

	public static bool Contains(this AttackTypeFlag mask, AttackType attackType)
	{
		return ((uint)mask & (uint)(1 << (int)attackType)) != 0;
	}

	public static AttackTypeFlag ToFlag(this AttackType attackType)
	{
		return (AttackTypeFlag)(1 << (int)attackType);
	}
}
