using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Items;

public static class WeaponStatsHelper
{
	private readonly struct Key : IEquatable<Key>
	{
		[CanBeNull]
		private readonly AbilityData m_Ability;

		[CanBeNull]
		private readonly ItemEntityWeapon m_Weapon;

		[CanBeNull]
		private readonly MechanicEntity m_Wielder;

		public Key([CanBeNull] AbilityData ability, [CanBeNull] ItemEntityWeapon weapon, [CanBeNull] MechanicEntity wielder)
		{
			m_Wielder = wielder;
			m_Weapon = weapon;
			m_Ability = ability;
		}

		public bool Equals(Key other)
		{
			if (object.Equals(m_Wielder, other.m_Wielder) && object.Equals(m_Weapon, other.m_Weapon))
			{
				return object.Equals(m_Ability, other.m_Ability);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is Key other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(m_Wielder, m_Weapon, m_Ability);
		}
	}

	private static readonly Dictionary<Key, RuleCalculateStatsWeapon> Cache = new Dictionary<Key, RuleCalculateStatsWeapon>(100);

	private static int s_LastFrame;

	[NotNull]
	public static RuleCalculateStatsWeapon GetWeaponStats(this ItemEntityWeapon weapon, MechanicEntity wielder = null, MechanicEntity target = null)
	{
		return GetWeaponStats(null, weapon, wielder ?? weapon.Wielder, target);
	}

	[NotNull]
	public static RuleCalculateStatsWeapon GetWeaponStats(this AbilityData ability, MechanicEntity wielder = null, MechanicEntity target = null)
	{
		return GetWeaponStats(ability, ability.Weapon, wielder ?? ability.Caster, target);
	}

	[NotNull]
	public static RuleCalculateStatsWeapon GetWeaponStats([CanBeNull] AbilityData ability, [CanBeNull] ItemEntityWeapon weapon, [CanBeNull] MechanicEntity wielder, [CanBeNull] MechanicEntity target)
	{
		if (wielder == null)
		{
			wielder = Game.Instance.DefaultUnit;
		}
		InvalidateCacheIfNecessary();
		Key key = new Key(ability, weapon, wielder);
		RuleCalculateStatsWeapon ruleCalculateStatsWeapon = Cache.Get(key);
		if (ruleCalculateStatsWeapon == null)
		{
			ruleCalculateStatsWeapon = Rulebook.Trigger(new RuleCalculateStatsWeapon(wielder, target, weapon, ability));
			Cache.Add(key, ruleCalculateStatsWeapon);
		}
		return ruleCalculateStatsWeapon;
	}

	private static void InvalidateCacheIfNecessary()
	{
		int currentSystemStepIndex = Game.Instance.RealTimeController.CurrentSystemStepIndex;
		if (s_LastFrame != currentSystemStepIndex)
		{
			s_LastFrame = currentSystemStepIndex;
			Cache.Clear();
		}
	}

	public static void ForceInvalidateCache()
	{
		s_LastFrame = 0;
		Cache.Clear();
	}
}
