using System;
using System.Collections.Generic;
using Kingmaker.Items;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Animation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartTwoHandedInOneHand : UnitPart, IHashable
{
	private readonly struct Entry : IEquatable<Entry>
	{
		public readonly UnitFact Fact;

		public readonly TwoHandedWeaponsInOneHand Component;

		public Entry(UnitFact fact, TwoHandedWeaponsInOneHand component)
		{
			Fact = fact;
			Component = component;
		}

		public bool Equals(Entry other)
		{
			if (Fact == other.Fact)
			{
				return Component == other.Component;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Entry other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(Fact, Component);
		}
	}

	private readonly List<Entry> m_Entries = new List<Entry>();

	public void Register(UnitFact fact, TwoHandedWeaponsInOneHand component)
	{
		m_Entries.Add(new Entry(fact, component));
	}

	public void Unregister(UnitFact fact, TwoHandedWeaponsInOneHand component)
	{
		m_Entries.Remove(new Entry(fact, component));
	}

	public WeaponAnimationStyle? GetAnimationStyle(ItemEntityWeapon weapon)
	{
		foreach (Entry entry in m_Entries)
		{
			if (entry.Component.AppliesToWeapon(weapon))
			{
				return entry.Component.TargetAnimationStyle;
			}
		}
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
