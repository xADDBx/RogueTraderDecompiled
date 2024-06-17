using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.StateHasher.Hashers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.DotNetExtensions;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartSpellResistance : BaseUnitPart, IHashable
{
	public class SpellResistanceValue : IHashable
	{
		[JsonProperty]
		public readonly int Id;

		[JsonProperty]
		public readonly string FactId;

		[JsonProperty]
		public int Value;

		[JsonProperty]
		public SpellDescriptor? SpellDescriptor;

		[JsonConstructor]
		public SpellResistanceValue(int id, string factId)
		{
			FactId = factId;
			Id = id;
		}

		public bool CanApply([CanBeNull] MechanicsContext context)
		{
			return CanApply(context?.SourceAbility, context?.MaybeCaster, context?.SpellDescriptor);
		}

		public bool CanApply([CanBeNull] BlueprintAbility ability, [CanBeNull] MechanicEntity caster, SpellDescriptor? contextDescriptors)
		{
			if (ability != null && !ability.SpellResistance)
			{
				return false;
			}
			if (SpellDescriptor.HasValue && (ability == null || !ability.SpellDescriptor.HasAnyFlag(SpellDescriptor.Value)) && (!contextDescriptors.HasValue || !contextDescriptors.Value.HasAnyFlag(SpellDescriptor.Value)))
			{
				return false;
			}
			return true;
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			int val = Id;
			result.Append(ref val);
			result.Append(FactId);
			result.Append(ref Value);
			if (SpellDescriptor.HasValue)
			{
				SpellDescriptor val2 = SpellDescriptor.Value;
				result.Append(ref val2);
			}
			return result;
		}
	}

	public class SpellImmunity : IHashable
	{
		[JsonProperty]
		public readonly int Id;

		[JsonProperty]
		public SpellImmunityType Type;

		[JsonProperty]
		[CanBeNull]
		public BlueprintAbility[] Exceptions;

		[JsonProperty]
		public SpellDescriptor SpellDescriptor;

		[JsonProperty]
		public BlueprintUnitFact CasterIgnoreImmunityFact;

		[JsonConstructor]
		public SpellImmunity(int id)
		{
			Id = id;
		}

		public bool CanApply([CanBeNull] MechanicsContext context)
		{
			return CanApply(context?.SourceAbility, context?.MaybeCaster, context?.SpellDescriptor);
		}

		public bool CanApply([CanBeNull] BlueprintAbility ability, [CanBeNull] MechanicEntity caster, SpellDescriptor? contextDescriptors)
		{
			if ((bool)CasterIgnoreImmunityFact && caster != null && caster.Facts.Contains(CasterIgnoreImmunityFact))
			{
				return false;
			}
			switch (Type)
			{
			case SpellImmunityType.Simple:
				if (!Exceptions.HasItem(ability))
				{
					return ability?.SpellResistance ?? true;
				}
				return false;
			case SpellImmunityType.Specific:
				return Exceptions.HasItem(ability);
			case SpellImmunityType.SingleTarget:
				if (ability != null && !ability.GetComponent<AbilityTargetsInPattern>() && !ability.GetComponent<AbilityDeliverChain>() && !ability.GetComponent<AbilityDeliverAttackWithWeapon>())
				{
					return ability.PatternSettings == null;
				}
				return false;
			case SpellImmunityType.SpellDescriptor:
				if (contextDescriptors.HasValue)
				{
					return contextDescriptors.Value.HasAnyFlag(SpellDescriptor);
				}
				if (ability != null && ability.SpellDescriptor.HasAnyFlag(SpellDescriptor))
				{
					return !Exceptions.HasItem(ability);
				}
				return false;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public virtual Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			int val = Id;
			result.Append(ref val);
			result.Append(ref Type);
			BlueprintAbility[] exceptions = Exceptions;
			if (exceptions != null)
			{
				for (int i = 0; i < exceptions.Length; i++)
				{
					Hash128 val2 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(exceptions[i]);
					result.Append(ref val2);
				}
			}
			result.Append(ref SpellDescriptor);
			Hash128 val3 = Kingmaker.StateHasher.Hashers.SimpleBlueprintHasher.GetHash128(CasterIgnoreImmunityFact);
			result.Append(ref val3);
			return result;
		}
	}

	[JsonProperty]
	private int m_NextId;

	[JsonProperty]
	private readonly List<SpellResistanceValue> m_SRs = new List<SpellResistanceValue>();

	[JsonProperty]
	private readonly List<SpellImmunity> m_SpellImmunities = new List<SpellImmunity>();

	[JsonProperty]
	private int AllSRPenalty;

	public IList<SpellResistanceValue> SRs => m_SRs;

	public IList<SpellImmunity> Immunities => m_SpellImmunities;

	public int GetValue([CanBeNull] MechanicsContext context)
	{
		int num = 0;
		foreach (SpellResistanceValue sR in m_SRs)
		{
			if (sR.CanApply(context))
			{
				num = Math.Max(num, sR.Value);
			}
		}
		return num - AllSRPenalty;
	}

	public int GetValue(BlueprintAbility ability, [CanBeNull] MechanicEntity caster)
	{
		int num = 0;
		foreach (SpellResistanceValue sR in m_SRs)
		{
			if (sR.CanApply(ability, caster, null))
			{
				num = Math.Max(num, sR.Value);
			}
		}
		return num;
	}

	public bool IsImmune([CanBeNull] MechanicsContext context)
	{
		foreach (SpellImmunity spellImmunity in m_SpellImmunities)
		{
			if (spellImmunity.CanApply(context))
			{
				return true;
			}
		}
		return false;
	}

	public bool IsImmune(BlueprintAbility ability, [CanBeNull] MechanicEntity caster)
	{
		foreach (SpellImmunity spellImmunity in m_SpellImmunities)
		{
			if (spellImmunity.CanApply(ability, caster, null))
			{
				return true;
			}
		}
		return false;
	}

	public int AddResistance(int resistance, string factId, AlignmentComponent? alignment, SpellDescriptor? spellDescriptor)
	{
		SpellResistanceValue spellResistanceValue = new SpellResistanceValue(m_NextId++, factId)
		{
			Value = resistance,
			SpellDescriptor = spellDescriptor
		};
		m_SRs.Add(spellResistanceValue);
		return spellResistanceValue.Id;
	}

	public int AddImmunity(SpellImmunityType type, IEnumerable<BlueprintAbility> exceptions, SpellDescriptor spellDescriptor, BlueprintUnitFact casterIgnoreImmunityFact)
	{
		SpellImmunity spellImmunity = new SpellImmunity(m_NextId++)
		{
			Type = type,
			Exceptions = exceptions.EmptyIfNull().ToArray(),
			SpellDescriptor = spellDescriptor,
			CasterIgnoreImmunityFact = casterIgnoreImmunityFact
		};
		m_SpellImmunities.Add(spellImmunity);
		return spellImmunity.Id;
	}

	public void Remove(int id)
	{
		m_SRs.RemoveAll((SpellResistanceValue i) => i.Id == id);
		m_SpellImmunities.RemoveAll((SpellImmunity i) => i.Id == id);
		if (m_SRs.Count == 0 && m_SpellImmunities.Count == 0)
		{
			base.Owner.Remove<UnitPartSpellResistance>();
		}
	}

	public void SetAllSRPenalty(int penalty)
	{
		AllSRPenalty = penalty;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		result.Append(ref m_NextId);
		List<SpellResistanceValue> sRs = m_SRs;
		if (sRs != null)
		{
			for (int i = 0; i < sRs.Count; i++)
			{
				Hash128 val2 = ClassHasher<SpellResistanceValue>.GetHash128(sRs[i]);
				result.Append(ref val2);
			}
		}
		List<SpellImmunity> spellImmunities = m_SpellImmunities;
		if (spellImmunities != null)
		{
			for (int j = 0; j < spellImmunities.Count; j++)
			{
				Hash128 val3 = ClassHasher<SpellImmunity>.GetHash128(spellImmunities[j]);
				result.Append(ref val3);
			}
		}
		result.Append(ref AllSRPenalty);
		return result;
	}
}
