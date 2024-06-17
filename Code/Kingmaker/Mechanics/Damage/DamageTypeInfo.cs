using Kingmaker.UnitLogic.Mechanics.Damage;

namespace Kingmaker.Mechanics.Damage;

public class DamageTypeInfo
{
	private readonly DamageCategory m_Category;

	public readonly DamageType Value;

	public readonly DamageTypeMask Mask;

	public DamageCategoryInfo Category => m_Category.GetInfo();

	public DamageCategoryMask CategoryMask => Category.Mask;

	public bool IgnoreArmor => Category.IgnoreArmor;

	public DamageTypeInfo(DamageType value, DamageCategory category)
	{
		Value = value;
		Mask = (DamageTypeMask)(1 << (int)value);
		m_Category = category;
	}

	public static implicit operator DamageType(DamageTypeInfo info)
	{
		return info.Value;
	}
}
