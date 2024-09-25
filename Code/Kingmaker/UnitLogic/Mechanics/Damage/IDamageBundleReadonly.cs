using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Items;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

public interface IDamageBundleReadonly : IEnumerable<DamageData>, IEnumerable
{
	[CanBeNull]
	DamageData WeaponDamage { get; }

	[CanBeNull]
	ItemEntityWeapon Weapon { get; }

	[CanBeNull]
	DamageData First { get; }
}
