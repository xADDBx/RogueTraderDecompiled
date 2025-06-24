using JetBrains.Annotations;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties;

public readonly struct PropertyContext
{
	[NotNull]
	public readonly MechanicEntity CurrentEntity;

	[CanBeNull]
	public readonly MechanicEntity CurrentTarget;

	[CanBeNull]
	private readonly TargetWrapper m_CurrentTargetWrapper;

	[CanBeNull]
	public readonly MechanicsContext MechanicContext;

	[CanBeNull]
	public readonly RulebookEvent Rule;

	[CanBeNull]
	public readonly AbilityData Ability;

	[CanBeNull]
	public readonly MechanicEntityFact Fact;

	[CanBeNull]
	public Vector3? CurrentTargetPosition
	{
		get
		{
			TargetWrapper currentTargetWrapper = m_CurrentTargetWrapper;
			if ((object)currentTargetWrapper == null)
			{
				return CurrentTarget?.Position;
			}
			return currentTargetWrapper.Point;
		}
	}

	[CanBeNull]
	public float? CurrentTargetOrientation
	{
		get
		{
			TargetWrapper currentTargetWrapper = m_CurrentTargetWrapper;
			if ((object)currentTargetWrapper == null)
			{
				return CurrentTarget?.Orientation;
			}
			return currentTargetWrapper.Orientation;
		}
	}

	[CanBeNull]
	public MechanicEntity ContextMainTarget => ContextMainTargetWrapper?.Entity;

	[CanBeNull]
	public Vector3? ContextMainTargetPosition => ContextMainTargetWrapper?.Point;

	[CanBeNull]
	public float? ContextMainTargetOrientation => ContextMainTargetWrapper?.Orientation;

	[CanBeNull]
	public (Vector3 Position, float Orientation)? ContextMainTargetPositionAndOrientation
	{
		get
		{
			if (!ContextMainTargetPosition.HasValue || !ContextMainTargetOrientation.HasValue)
			{
				return null;
			}
			return (ContextMainTargetPosition.Value, ContextMainTargetOrientation.Value);
		}
	}

	[CanBeNull]
	public MechanicEntity ContextCaster => MechanicContext?.MaybeCaster;

	[CanBeNull]
	private TargetWrapper ContextMainTargetWrapper => MechanicContext?.MainTarget;

	[CanBeNull]
	public MechanicEntity RuleInitiator => Rule?.Initiator as MechanicEntity;

	[CanBeNull]
	public MechanicEntity RuleTarget => Rule?.GetRuleTarget() as MechanicEntity;

	public PropertyContext([NotNull] MechanicEntity currentEntity, [CanBeNull] MechanicEntityFact fact, [CanBeNull] MechanicEntity currentTarget = null, [CanBeNull] MechanicsContext mechanicContext = null, [CanBeNull] RulebookEvent rule = null, [CanBeNull] AbilityData ability = null)
	{
		CurrentEntity = currentEntity;
		Fact = fact ?? (ContextData<EntityFactComponentDelegateContextData>.Current?.Runtime.Fact as MechanicEntityFact);
		MechanicContext = mechanicContext ?? ContextData<MechanicsContext.Data>.Current?.Context ?? fact?.MaybeContext;
		m_CurrentTargetWrapper = ((currentTarget != null) ? null : ContextData<MechanicsContext.Data>.Current?.CurrentTarget);
		CurrentTarget = currentTarget ?? m_CurrentTargetWrapper?.Entity ?? MechanicContext?.MainTarget.Entity;
		Rule = rule ?? (ContextData<RulebookContextData>.Current?.Rule as RulebookEvent);
		Ability = ability ?? Rule?.Reason.Ability;
	}

	public PropertyContext(AbilityData ability, MechanicEntity currentTarget)
		: this(ability.Caster, ability.Fact, currentTarget, ability.Fact?.Context, null, ability)
	{
	}

	public PropertyContext([NotNull] MechanicEntityFact fact, [CanBeNull] MechanicEntity currentTarget = null, [CanBeNull] RulebookEvent rule = null, [CanBeNull] AbilityData ability = null)
		: this(fact.ConcreteOwner, fact, currentTarget, fact.MaybeContext, rule, ability)
	{
	}

	private PropertyContext([NotNull] MechanicEntity currentEntity, [CanBeNull] MechanicEntityFact fact, [CanBeNull] MechanicEntity currentTarget, [CanBeNull] TargetWrapper currentTargetWrapper, [CanBeNull] MechanicsContext mechanicContext, [CanBeNull] RulebookEvent rule, [CanBeNull] AbilityData ability)
	{
		CurrentEntity = currentEntity;
		Fact = fact;
		CurrentTarget = currentTarget;
		m_CurrentTargetWrapper = currentTargetWrapper;
		MechanicContext = mechanicContext;
		Rule = rule;
		Ability = ability;
	}

	public int GetValue(PropertyCalculator calculator)
	{
		return calculator.GetValue(this);
	}

	public bool GetBool(PropertyCalculator calculator)
	{
		return calculator.GetValue(this) != 0;
	}

	public PropertyContext WithCurrentEntity([NotNull] MechanicEntity currentEntity)
	{
		return new PropertyContext(currentEntity, Fact, CurrentTarget, m_CurrentTargetWrapper, MechanicContext, Rule, Ability);
	}

	public PropertyContext WithFact([NotNull] MechanicEntityFact fact)
	{
		return new PropertyContext(CurrentEntity, fact, CurrentTarget, m_CurrentTargetWrapper, MechanicContext, Rule, Ability);
	}

	public PropertyContext WithCurrentTarget([NotNull] MechanicEntity currentTarget)
	{
		return new PropertyContext(CurrentEntity, Fact, currentTarget, null, MechanicContext, Rule, Ability);
	}

	public PropertyContext WithContext([NotNull] MechanicsContext context)
	{
		return new PropertyContext(CurrentEntity, Fact, CurrentTarget, m_CurrentTargetWrapper, context, Rule, Ability);
	}

	public PropertyContext WithRule([NotNull] RulebookEvent rule)
	{
		return new PropertyContext(CurrentEntity, Fact, CurrentTarget, m_CurrentTargetWrapper, MechanicContext, rule, Ability);
	}

	public PropertyContext WithAbility([NotNull] AbilityData ability)
	{
		return new PropertyContext(CurrentEntity, Fact, CurrentTarget, m_CurrentTargetWrapper, MechanicContext, Rule, ability);
	}
}
