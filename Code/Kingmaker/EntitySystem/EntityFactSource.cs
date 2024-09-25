using System;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.AreaLogic.Etudes;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Progression.Paths;
using Kingmaker.Utility.UnityExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public class EntityFactSource : IHashable
{
	public static readonly EntityFactSource Empty = new EntityFactSource();

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private readonly EntityRef m_EntityRef;

	[JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
	private readonly EntityFactRef m_FactRef;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private readonly string m_ComponentId;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private readonly UnitCondition? m_UnitCondition;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private readonly BlueprintScriptableObject m_Blueprint;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private readonly BlueprintScriptableObject m_PathFeatureSource;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private readonly int? m_PathRank;

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	[CanBeNull]
	private readonly int? m_FeatureRank;

	[CanBeNull]
	public EntityFact Fact => m_FactRef.Fact;

	[CanBeNull]
	public Entity Entity => ((Entity)m_EntityRef.Entity) ?? m_FactRef.Entity;

	[CanBeNull]
	public Etude Etude => Fact as Etude;

	[CanBeNull]
	public UnitCondition? UnitCondition => m_UnitCondition;

	[CanBeNull]
	public BlueprintScriptableObject Blueprint => Fact?.Blueprint ?? m_Blueprint;

	[CanBeNull]
	public BlueprintScriptableObject PathFeatureSource => m_PathFeatureSource;

	[CanBeNull]
	public BlueprintScriptableObject BlueprintRaw => m_Blueprint;

	[CanBeNull]
	public Cutscene Cutscene => m_Blueprint as Cutscene;

	[CanBeNull]
	public BlueprintPath Path => m_Blueprint as BlueprintPath;

	[CanBeNull]
	public int? PathRank => m_PathRank;

	[CanBeNull]
	public int? FeatureRank => m_FeatureRank;

	public bool IsMissing
	{
		get
		{
			if (Cutscene == null)
			{
				if (m_EntityRef.IsEmpty || m_EntityRef.Entity != null)
				{
					if (!m_FactRef.IsEmpty)
					{
						return m_FactRef.Fact == null;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	[JsonConstructor]
	private EntityFactSource([CanBeNull] Entity _entity = null, [CanBeNull] EntityFact _fact = null, [CanBeNull] BlueprintComponent _component = null, [CanBeNull] UnitCondition? _unitCondition = null, [CanBeNull] BlueprintScriptableObject _blueprint = null, [CanBeNull] BlueprintScriptableObject _pathFeatureSource = null, [CanBeNull] int? _pathRank = null, [CanBeNull] int? _featureRank = null)
	{
		m_FactRef = _fact;
		m_ComponentId = _component?.name;
		m_EntityRef = _entity?.Ref ?? default(EntityRef);
		m_UnitCondition = _unitCondition;
		m_Blueprint = _blueprint;
		m_PathFeatureSource = _pathFeatureSource;
		m_PathRank = _pathRank;
		m_FeatureRank = _featureRank;
	}

	public EntityFactSource([NotNull] EntityFact fact, [CanBeNull] BlueprintComponent component)
		: this(null, fact, component)
	{
	}

	public EntityFactSource([NotNull] Entity entity)
		: this(entity, null, null, null, null, null, null, null)
	{
	}

	public EntityFactSource([NotNull] CutscenePlayerData cutscene)
		: this(cutscene.Cutscene)
	{
	}

	public EntityFactSource(UnitCondition unitCondition)
		: this(null, null, null, unitCondition)
	{
	}

	public EntityFactSource([NotNull] Etude etude)
		: this(etude, null)
	{
	}

	public EntityFactSource([NotNull] BlueprintScriptableObject blueprint, int? pathRank = null)
		: this(null, null, null, null, blueprint, null, pathRank)
	{
	}

	public EntityFactSource([NotNull] BlueprintPath path, [CanBeNull] BlueprintScriptableObject pathFeatureSource, int pathRank, int featureRank)
		: this(null, null, null, null, path, pathFeatureSource, pathRank, featureRank)
	{
	}

	public bool IsFrom([NotNull] EntityFact fact, [CanBeNull] BlueprintComponent component = null)
	{
		if (m_FactRef.Entity?.UniqueId == fact.Owner?.UniqueId && m_FactRef.FactId == fact.UniqueId)
		{
			if (component != null)
			{
				return component.name == m_ComponentId;
			}
			return true;
		}
		return false;
	}

	public bool IsFrom([NotNull] Entity entity)
	{
		if (!(m_EntityRef.Entity?.UniqueId == entity.UniqueId))
		{
			return m_FactRef.Entity?.UniqueId == entity.UniqueId;
		}
		return true;
	}

	public bool IsFrom(UnitCondition unitCondition)
	{
		return m_UnitCondition == unitCondition;
	}

	public bool IsFrom(Etude etude)
	{
		return Etude == etude;
	}

	public bool Equals(EntityFactSource other)
	{
		if ((object)this != other)
		{
			if ((object)other != null && m_EntityRef.Equals(other.m_EntityRef) && m_FactRef.Equals(other.m_FactRef) && m_ComponentId == other.m_ComponentId && m_UnitCondition == other.m_UnitCondition && m_Blueprint == other.m_Blueprint)
			{
				return m_PathRank == other.m_PathRank;
			}
			return false;
		}
		return true;
	}

	public override bool Equals(object obj)
	{
		if (obj is EntityFactSource other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(m_EntityRef, m_FactRef, m_ComponentId, m_UnitCondition, m_Blueprint, m_PathFeatureSource, m_PathRank);
	}

	public static bool operator ==(EntityFactSource o1, EntityFactSource o2)
	{
		return o1?.Equals(o2) ?? ((object)o2 == null);
	}

	public static bool operator !=(EntityFactSource o1, EntityFactSource o2)
	{
		if ((object)o1 == null)
		{
			return (object)o2 != null;
		}
		return !o1.Equals(o2);
	}

	public override string ToString()
	{
		if (!m_FactRef.IsEmpty)
		{
			if (!m_ComponentId.IsNullOrEmpty())
			{
				return $"FactSource[{m_FactRef.Fact}]({m_ComponentId})";
			}
			return $"FactSource[{m_FactRef.Fact}]";
		}
		if (!m_EntityRef.IsEmpty)
		{
			return $"FactSource[{m_EntityRef.Entity}]";
		}
		if (m_UnitCondition.HasValue)
		{
			return $"FactSource[{m_UnitCondition.Value}]";
		}
		if (m_Blueprint != null)
		{
			if (m_PathRank.HasValue)
			{
				return $"FactSource[{m_Blueprint}]({m_PathRank.Value})";
			}
			return $"FactSource[{m_Blueprint}]";
		}
		return "FactSource[???]";
	}

	public virtual Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		EntityRef obj = m_EntityRef;
		Hash128 val = EntityRefHasher.GetHash128(ref obj);
		result.Append(ref val);
		EntityFactRef obj2 = m_FactRef;
		Hash128 val2 = StructHasher<EntityFactRef>.GetHash128(ref obj2);
		result.Append(ref val2);
		result.Append(m_ComponentId);
		if (m_UnitCondition.HasValue)
		{
			UnitCondition val3 = m_UnitCondition.Value;
			result.Append(ref val3);
		}
		Hash128 val4 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_Blueprint);
		result.Append(ref val4);
		Hash128 val5 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(m_PathFeatureSource);
		result.Append(ref val5);
		if (m_PathRank.HasValue)
		{
			int val6 = m_PathRank.Value;
			result.Append(ref val6);
		}
		if (m_FeatureRank.HasValue)
		{
			int val7 = m_FeatureRank.Value;
			result.Append(ref val7);
		}
		return result;
	}
}
