using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.Designers.Mechanics.Facts.Restrictions;

[Serializable]
public class RestrictionCalculator
{
	public PropertyCalculator Property;

	public bool IsPassed([NotNull] MechanicEntityFact fact, [CanBeNull] RulebookEvent rule, [CanBeNull] AbilityData ability = null)
	{
		PropertyContext context = new PropertyContext(fact, null, rule, ability);
		return IsPassedInternal(context);
	}

	public bool IsPassed([NotNull] MechanicEntityFact fact, [CanBeNull] MechanicsContext mechanicsContext, [CanBeNull] RulebookEvent rule = null, [CanBeNull] AbilityData ability = null, [CanBeNull] MechanicEntity target = null)
	{
		PropertyContext context = new PropertyContext(fact.ConcreteOwner, fact, target, mechanicsContext, rule, ability);
		return IsPassedInternal(context);
	}

	public bool IsPassed([NotNull] MechanicsContext context, [NotNull] MechanicEntity currentEntity, [CanBeNull] MechanicEntityFact fact = null, [CanBeNull] MechanicEntity target = null)
	{
		PropertyContext context2 = new PropertyContext(currentEntity, fact, target, context, null, context.SourceAbilityContext?.Ability);
		return IsPassedInternal(context2);
	}

	public bool IsPassed([NotNull] MechanicEntityFact fact)
	{
		return IsPassedInternal(new PropertyContext(fact));
	}

	public bool IsPassed([NotNull] MechanicEntityFact fact, [NotNull] MechanicEntity target)
	{
		return IsPassedInternal(new PropertyContext(fact, target));
	}

	public bool IsPassed(PropertyContext context)
	{
		return IsPassedInternal(context);
	}

	private bool IsPassedInternal(PropertyContext context)
	{
		if (Property == null)
		{
			return true;
		}
		if (!Property.Empty)
		{
			return Property.GetBoolValue(context);
		}
		return true;
	}
}
