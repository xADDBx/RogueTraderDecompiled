using System;
using System.Collections.Generic;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.AI.Learning.Collections;

[Serializable]
public class AttackDataCollection : ListBasedDataCollection<AttackData>
{
	[HashRoot]
	public class AbilityThreatData : IHashable
	{
		public readonly string Ability;

		public int TotalDamage;

		public int MaxRange;

		public AbilityThreatData(string ability, int totalDamage, int maxRange)
		{
			Ability = ability;
			TotalDamage = totalDamage;
			MaxRange = maxRange;
		}

		public override string ToString()
		{
			return $"{GetType().Name}[{Ability}, MaxRange: {MaxRange}, TotalDamage: {TotalDamage}]";
		}

		public virtual Hash128 GetHash128()
		{
			return default(Hash128);
		}
	}

	public static class Hasher
	{
		[HasherFor(Type = typeof(AttackDataCollection))]
		public static Hash128 GetHash128(AttackDataCollection obj)
		{
			Hash128 result = default(Hash128);
			if (obj == null)
			{
				return result;
			}
			foreach (AttackData item in obj.m_Collection)
			{
				Hash128 val = item.GetHash128();
				result.Append(ref val);
			}
			foreach (AbilityThreatData abilityThreatData in obj.m_AbilityThreatDatas)
			{
				Hash128 val2 = ClassHasher<AbilityThreatData>.GetHash128(abilityThreatData);
				result.Append(ref val2);
			}
			return result;
		}
	}

	private List<AbilityThreatData> m_AbilityThreatDatas = new List<AbilityThreatData>();

	public override void Add(AttackData item)
	{
		base.Add(item);
		AbilityThreatData abilityThreatData = m_AbilityThreatDatas.FirstOrDefault((AbilityThreatData i) => i.Ability == item.Ability);
		if (abilityThreatData == null)
		{
			m_AbilityThreatDatas.Add(new AbilityThreatData(item.Ability, item.Damage, item.Range));
			return;
		}
		abilityThreatData.TotalDamage += item.Damage;
		abilityThreatData.MaxRange = Math.Max(abilityThreatData.MaxRange, item.Range);
	}

	public override void Clear()
	{
		base.Clear();
		m_AbilityThreatDatas.Clear();
	}

	public int GetThreatRange()
	{
		int result = 0;
		AbilityThreatData abilityThreatData = m_AbilityThreatDatas.MaxBy((AbilityThreatData d) => d.TotalDamage);
		if (abilityThreatData != null)
		{
			result = abilityThreatData.MaxRange;
		}
		return result;
	}
}
