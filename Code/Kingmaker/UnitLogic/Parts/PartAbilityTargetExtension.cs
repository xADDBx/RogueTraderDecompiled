using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilityTargetExtension : BaseUnitPart, IHashable
{
	private readonly struct Entry : IEquatable<Entry>, IHashable
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference<FeatureAllowAdditionalTargetTypes> _componentRef;

		public EntityFact Fact => _factRef;

		public FeatureAllowAdditionalTargetTypes Component => _componentRef.Get();

		public Entry(EntityFact fact, FeatureAllowAdditionalTargetTypes component)
		{
			_factRef = fact;
			_componentRef = component;
		}

		public bool Equals(Entry other)
		{
			if (_factRef.Equals(other._factRef))
			{
				return _componentRef.Equals(other._componentRef);
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
			return HashCode.Combine(_factRef, _componentRef);
		}

		public Hash128 GetHash128()
		{
			return default(Hash128);
		}
	}

	[JsonProperty]
	private readonly HashSet<Entry> Entries = new HashSet<Entry>();

	public bool CanTargetType(AbilityData abilityData, IAbilityAllowTargetingType.TargetTypeEnum targetType)
	{
		foreach (Entry entry in Entries)
		{
			if (entry.Component.TargetType == targetType && entry.Component.IsRestrictionPassed(abilityData))
			{
				return true;
			}
		}
		return false;
	}

	public void Register(EntityFact fact, FeatureAllowAdditionalTargetTypes component)
	{
		if (!Entries.Add(new Entry(fact, component)))
		{
			PFLog.Ability.Error($"Failed to add entry for fact {fact} and component {component}. It has already been added");
		}
	}

	public void Unregister(EntityFact fact, FeatureAllowAdditionalTargetTypes component)
	{
		Entries.Remove(new Entry(fact, component));
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		HashSet<Entry> entries = Entries;
		if (entries != null)
		{
			int num = 0;
			foreach (Entry item in entries)
			{
				Entry obj = item;
				num ^= StructHasher<Entry>.GetHash128(ref obj).GetHashCode();
			}
			result.Append(num);
		}
		return result;
	}
}
