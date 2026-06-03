using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class PartAbilityImmunity : UnitPart, ITargetRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, ITargetRulebookSubscriber, ITargetRulebookHandler<RuleCalculateHeal>, IRulebookHandler<RuleCalculateHeal>, ITargetRulebookHandler<RuleCalculateCanApplyBuff>, IRulebookHandler<RuleCalculateCanApplyBuff>, IHashable
{
	private readonly struct Entry : IEquatable<Entry>
	{
		private readonly EntityFactRef _factRef;

		private readonly BlueprintComponentReference<AbilityImmunityComponent> _componentRef;

		public EntityFact Fact => _factRef;

		public AbilityImmunityComponent Component => _componentRef.Get();

		public Entry(EntityFact fact, AbilityImmunityComponent component)
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
	}

	private readonly HashSet<Entry> Entries = new HashSet<Entry>();

	public bool IsImmuneTo(BlueprintAbility blueprintAbility)
	{
		if (blueprintAbility == null)
		{
			return false;
		}
		foreach (Entry entry in Entries)
		{
			if (entry.Component.HasImmunityTo(blueprintAbility))
			{
				return true;
			}
		}
		return false;
	}

	public void Register(EntityFact fact, AbilityImmunityComponent component)
	{
		if (!Entries.Add(new Entry(fact, component)))
		{
			throw new InvalidOperationException("Already registered");
		}
	}

	public void Unregister(EntityFact fact, AbilityImmunityComponent component)
	{
		Entries.Remove(new Entry(fact, component));
	}

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (IsImmuneTo(evt.Reason.Ability?.Blueprint))
		{
			evt.ValueModifiers.Add(ModifierType.PctMul_Extra, 0, ModifierDescriptor.Immunity);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateCanApplyBuff evt)
	{
		if (IsImmuneTo(evt.Reason.Ability?.Blueprint))
		{
			evt.Immunity = true;
		}
	}

	public void OnEventDidTrigger(RuleCalculateCanApplyBuff evt)
	{
	}

	public void OnEventAboutToTrigger(RuleCalculateHeal evt)
	{
		if (IsImmuneTo(evt.Reason.Ability?.Blueprint))
		{
			evt.Nullify = true;
		}
	}

	public void OnEventDidTrigger(RuleCalculateHeal evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
