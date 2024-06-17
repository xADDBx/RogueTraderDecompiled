using System.Linq;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Mechanics.Damage;

public class DamageCategoryInfo
{
	private readonly DamageType[] m_DamageTypes;

	public readonly DamageCategory Value;

	public readonly DamageCategoryMask Mask;

	public readonly DamageTypeMask DamageTypesMask;

	public readonly bool IgnoreArmor;

	public ReadonlyList<DamageType> DamageTypes => m_DamageTypes;

	public DamageCategoryInfo(DamageCategory value, DamageTypeMask typesMask, bool ignoreArmor)
	{
		Value = value;
		DamageTypesMask = typesMask;
		Mask = (DamageCategoryMask)(1 << (int)value);
		IgnoreArmor = ignoreArmor;
		m_DamageTypes = (from i in Enumerable.Range(0, 32)
			where ((uint)typesMask & (uint)(1 << i)) != 0
			select (DamageType)i).ToArray();
	}
}
