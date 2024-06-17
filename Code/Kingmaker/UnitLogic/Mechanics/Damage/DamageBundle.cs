using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Items;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Mechanics.Damage;

public class DamageBundle : IDamageBundleReadonly, IEnumerable<DamageData>, IEnumerable
{
	private readonly List<DamageData> m_Chunks = new List<DamageData>();

	public ItemEntityWeapon Weapon { get; set; }

	public DamageData First => m_Chunks.FirstItem();

	public DamageData WeaponDamage
	{
		get
		{
			if (Weapon == null)
			{
				return null;
			}
			return First;
		}
	}

	public DamageBundle(params DamageData[] damages)
	{
		foreach (DamageData damage in damages)
		{
			Add(damage);
		}
	}

	public DamageBundle(ItemEntityWeapon weapon, DamageData damages)
		: this(damages)
	{
		Weapon = weapon;
	}

	public void Add(DamageData damage)
	{
		m_Chunks.Add(damage);
	}

	public void Remove(Predicate<DamageData> pred)
	{
		m_Chunks.RemoveAll(pred);
	}

	public IEnumerator<DamageData> GetEnumerator()
	{
		return m_Chunks.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
