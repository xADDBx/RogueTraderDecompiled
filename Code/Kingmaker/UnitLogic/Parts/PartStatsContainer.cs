using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartStatsContainer : MechanicEntityPart, IHashable
{
	public interface IOwner : IEntityPartOwner<PartStatsContainer>, IEntityPartOwner
	{
		PartStatsContainer Stats { get; }
	}

	public readonly struct StatOverrideData
	{
		public StatType OverrideType { get; }

		public BlueprintComponentReference<ReplaceStat> Source { get; }

		public EntityFactRef FactRef { get; }

		public EntityFact Fact => FactRef;

		public StatOverrideData(StatType overrideType, [NotNull] ReplaceStat source, EntityFact fact)
		{
			OverrideType = overrideType;
			Source = source;
			FactRef = fact;
		}

		public override bool Equals(object obj)
		{
			if (obj is StatOverrideData statOverrideData && OverrideType == statOverrideData.OverrideType && object.Equals(Source, statOverrideData.Source))
			{
				return object.Equals(FactRef, statOverrideData.FactRef);
			}
			return false;
		}

		public bool Equals(StatOverrideData other)
		{
			if (OverrideType == other.OverrideType && object.Equals(Source, other.Source))
			{
				return object.Equals(FactRef, other.FactRef);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(OverrideType, Source, FactRef);
		}

		public static bool operator ==(StatOverrideData left, StatOverrideData right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(StatOverrideData left, StatOverrideData right)
		{
			return !(left == right);
		}

		public bool CanBeUsed(MechanicEntity owner)
		{
			ReplaceStat replaceStat = Source.Get();
			if (replaceStat != null)
			{
				return replaceStat.RestrictionCalculator?.IsPassed(FactRef.Fact as MechanicEntityFact, owner) ?? true;
			}
			return false;
		}
	}

	private readonly Dictionary<StatType, List<StatOverrideData>> m_OverridenBaseStat = new Dictionary<StatType, List<StatOverrideData>>();

	private readonly Dictionary<StatType, List<StatOverrideData>> m_OverridenStats = new Dictionary<StatType, List<StatOverrideData>>();

	[JsonProperty]
	public StatsContainer Container { get; private set; }

	public IReadOnlyDictionary<StatType, List<StatOverrideData>> OverridenBaseStat => m_OverridenBaseStat;

	public IReadOnlyDictionary<StatType, List<StatOverrideData>> OverridenStats => m_OverridenStats;

	public IEnumerable<ModifiableValue> AllStats => Container.AllStats;

	protected override void OnAttach()
	{
		Container = new StatsContainer(base.Owner);
	}

	protected override void OnPrePostLoad()
	{
		Container.PrePostLoad(base.Owner);
	}

	protected override void OnPostLoad()
	{
		Container.PostLoad();
	}

	public void CleanupModifiers()
	{
		Container.CleanupModifiers();
	}

	public void AddClassSkill(StatType stat)
	{
		Container.AddClassSkill(stat);
	}

	[CanBeNull]
	public TModifiableValue GetStatOptional<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return Container.GetStatOptional<TModifiableValue>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStatOptional(StatType type)
	{
		return Container.GetStatOptional(type);
	}

	[CanBeNull]
	public ModifiableValueAttributeStat GetAttributeOptional(StatType type)
	{
		return Container.GetStatOptional<ModifiableValueAttributeStat>(type);
	}

	[CanBeNull]
	public ModifiableValueSkill GetSkillOptional(StatType type)
	{
		return Container.GetStatOptional<ModifiableValueSkill>(type);
	}

	[CanBeNull]
	public ModifiableValue GetStat(StatType type, bool canBeNull = false)
	{
		return Container.GetStat(type, canBeNull);
	}

	[NotNull]
	public TModifiableValue GetStat<TModifiableValue>(StatType type) where TModifiableValue : ModifiableValue
	{
		return Container.GetStat<TModifiableValue>(type);
	}

	[NotNull]
	public ModifiableValueAttributeStat GetAttribute(StatType type)
	{
		return Container.GetAttribute(type);
	}

	[NotNull]
	public ModifiableValueSkill GetSkill(StatType type)
	{
		return Container.GetSkill(type);
	}

	private void AddStatOverride(Dictionary<StatType, List<StatOverrideData>> statOverrides, StatType stat, StatType overrideStat, ReplaceStat source, MechanicEntityFact fact)
	{
		StatOverrideData item = new StatOverrideData(overrideStat, source, fact);
		if (!statOverrides.TryGetValue(stat, out var value))
		{
			value = (statOverrides[stat] = new List<StatOverrideData>());
		}
		if (!value.Contains(item))
		{
			value.Add(item);
		}
	}

	private void RemoveStatOverride(Dictionary<StatType, List<StatOverrideData>> statOverrides, StatType stat, ReplaceStat source)
	{
		if (!statOverrides.TryGetValue(stat, out var value))
		{
			return;
		}
		for (int num = value.Count - 1; num >= 0; num--)
		{
			if (value[num].Source.Get() == source)
			{
				value.RemoveAt(num);
			}
		}
		if (value.Count == 0)
		{
			statOverrides.Remove(stat);
		}
	}

	public void AddBaseStatOverride(StatType originalStat, StatType newStat, ReplaceStat source, MechanicEntityFact fact)
	{
		AddStatOverride(m_OverridenBaseStat, originalStat, newStat, source, fact);
	}

	public void RemoveBaseStatOverride(StatType originalStat, ReplaceStat source)
	{
		RemoveStatOverride(m_OverridenBaseStat, originalStat, source);
	}

	public void AddStatOverride(StatType originalStat, StatType newStat, ReplaceStat source, MechanicEntityFact fact)
	{
		AddStatOverride(m_OverridenStats, originalStat, newStat, source, fact);
	}

	public void RemoveStatOverride(StatType originalStat, ReplaceStat source)
	{
		RemoveStatOverride(m_OverridenStats, originalStat, source);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		Hash128 val2 = ClassHasher<StatsContainer>.GetHash128(Container);
		result.Append(ref val2);
		return result;
	}
}
