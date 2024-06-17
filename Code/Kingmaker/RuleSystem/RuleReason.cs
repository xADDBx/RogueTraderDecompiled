using System;
using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.Models.Tooltip.Base;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using UnityEngine;

namespace Kingmaker.RuleSystem;

public readonly struct RuleReason : IUIDataProvider, IEquatable<RuleReason>
{
	[CanBeNull]
	public MechanicEntityFact Fact { get; }

	[CanBeNull]
	public RulebookEvent Rule { get; }

	[CanBeNull]
	public ItemEntity Item { get; }

	[CanBeNull]
	public AbilityData Ability { get; }

	[CanBeNull]
	public MechanicEntity Caster { get; }

	[CanBeNull]
	public MechanicEntity SourceEntity { get; }

	[CanBeNull]
	public MechanicsContext Context { get; }

	[CanBeNull]
	public RulePerformAttack Attack => Rule as RulePerformAttack;

	[CanBeNull]
	public BaseUnitEntity SourceUnit => SourceEntity as BaseUnitEntity;

	public string Name => SelectUIData(UIDataType.Name)?.Name ?? "";

	public string Description => SelectUIData(UIDataType.Description)?.Description ?? "";

	public Sprite Icon => SelectUIData(UIDataType.Icon)?.Icon;

	public string NameForAcronym => SelectUIData(UIDataType.NameForAcronym)?.NameForAcronym ?? "";

	[CanBeNull]
	public IUIDataProvider SelectUIData(UIDataType type)
	{
		IUIDataProvider iUIDataProvider = Context?.SelectUIData(type);
		if (iUIDataProvider != null)
		{
			return iUIDataProvider;
		}
		iUIDataProvider = Fact?.SelectUIData(type);
		if (iUIDataProvider != null)
		{
			return iUIDataProvider;
		}
		if (Ability != null)
		{
			return Ability;
		}
		if (Item != null)
		{
			return Item;
		}
		return null;
	}

	public RuleReason([NotNull] MechanicEntity entity)
	{
		this = default(RuleReason);
		if (entity == null)
		{
			throw new ArgumentNullException("entity");
		}
		SourceEntity = entity;
		Context = (entity as AreaEffectEntity)?.Context;
	}

	public RuleReason([NotNull] MechanicEntityFact fact)
	{
		this = default(RuleReason);
		if (fact == null)
		{
			throw new ArgumentNullException("fact");
		}
		Fact = fact;
		Item = GetItemFromFact(fact);
		Caster = GetCaster(fact);
		SourceEntity = fact.ConcreteOwner;
		Context = fact.MaybeContext;
	}

	public RuleReason([NotNull] RulebookEvent rule)
	{
		this = default(RuleReason);
		if (rule == null)
		{
			throw new ArgumentNullException("rule");
		}
		SourceEntity = (Caster = (MechanicEntity)rule.Initiator);
		Rule = rule;
		if (rule is RulePerformAbility rulePerformAbility)
		{
			Ability = rulePerformAbility.Spell;
			Item = rulePerformAbility.Spell.SourceItem;
			Context = rulePerformAbility.Context;
		}
		else if (rule is RulePerformAttack rulePerformAttack)
		{
			Ability = rulePerformAttack.Ability;
			Item = rulePerformAttack.Ability.Weapon;
		}
	}

	public RuleReason([NotNull] MechanicsContext context)
	{
		this = default(RuleReason);
		if (context == null)
		{
			throw new ArgumentNullException("context");
		}
		MechanicEntityFact fact = ContextDataHelper.GetFact();
		MechanicsContext mechanicsContext = fact?.MaybeContext;
		if (mechanicsContext != null && mechanicsContext != context)
		{
			throw new ArgumentException("Fact context differs from current context", "context");
		}
		if (fact != null)
		{
			Fact = fact;
			Item = GetItemFromFact(fact);
		}
		Context = context;
		SourceEntity = (Caster = context.MaybeCaster);
		Ability = (context as AbilityExecutionContext)?.Ability;
	}

	public RuleReason(in RuleReason proto, [CanBeNull] MechanicsContext currentContext = null)
	{
		Context = currentContext ?? proto.Context;
		Fact = proto.Fact;
		Ability = proto.Ability;
		Rule = proto.Rule;
		Caster = proto.Caster;
		SourceEntity = proto.SourceEntity;
		Item = proto.Item;
	}

	public RuleReason Copy([CanBeNull] MechanicsContext currentContext)
	{
		return new RuleReason(in this, currentContext);
	}

	public static implicit operator RuleReason([NotNull] MechanicEntity entity)
	{
		return new RuleReason(entity);
	}

	public static implicit operator RuleReason([NotNull] MechanicEntityFact fact)
	{
		return new RuleReason(fact);
	}

	public static implicit operator RuleReason([NotNull] RulebookEvent rule)
	{
		return new RuleReason(rule);
	}

	public static implicit operator RuleReason([NotNull] MechanicsContext context)
	{
		return new RuleReason(context);
	}

	[CanBeNull]
	private static BaseUnitEntity GetSourceUnit(EntityFact fact)
	{
		return (fact.Owner as BaseUnitEntity) ?? ((fact.Owner as ItemEntity)?.Wielder as BaseUnitEntity);
	}

	[CanBeNull]
	private static MechanicEntity GetCaster(EntityFact fact)
	{
		return fact.MaybeContext?.MaybeCaster ?? GetSourceUnit(fact);
	}

	[CanBeNull]
	private static ItemEntity GetItemFromFact(EntityFact fact)
	{
		IItemEntity itemEntity = fact.Owner as ItemEntity;
		return (ItemEntity)(itemEntity ?? (fact as Ability)?.SourceItem);
	}

	public bool Equals(RuleReason other)
	{
		return this == other;
	}

	public override bool Equals(object obj)
	{
		if (obj is RuleReason other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return HashCode.Combine(Fact, Rule, Item, Ability, Caster, SourceEntity, Context);
	}

	public static bool operator ==(in RuleReason left, in RuleReason right)
	{
		if (left.Fact == right.Fact && left.Rule == right.Rule && left.Item == right.Item && left.Ability == right.Ability && left.Caster == right.Caster && left.SourceEntity == right.SourceEntity)
		{
			return left.Context == right.Context;
		}
		return false;
	}

	public static bool operator !=(in RuleReason left, in RuleReason right)
	{
		return !left.Equals(right);
	}
}
