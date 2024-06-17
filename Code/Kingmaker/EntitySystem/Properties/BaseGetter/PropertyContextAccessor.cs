using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Properties.BaseGetter;

public static class PropertyContextAccessor
{
	public interface IBase
	{
	}

	public interface IRequired : IBase
	{
	}

	public interface IOptional : IBase
	{
	}

	public interface IMechanicContext : IRequired, IBase
	{
	}

	public interface IRule : IRequired, IBase
	{
	}

	public interface IAbility : IRequired, IBase
	{
	}

	public interface IContextCaster : IRequired, IBase
	{
	}

	public interface IContextMainTargetEntity : IRequired, IBase
	{
	}

	public interface IContextMainTarget : IRequired, IBase
	{
	}

	public interface ICurrentTargetEntity : IRequired, IBase
	{
	}

	public interface ICurrentTarget : IRequired, IBase
	{
	}

	public interface IRuleInitiator : IRequired, IBase
	{
	}

	public interface IRuleTarget : IRequired, IBase
	{
	}

	public interface IAbilityWeapon : IRequired, IBase
	{
	}

	public interface ITargetByType : IRequired, IBase
	{
	}

	public interface IOwnerBlueprint : IRequired, IBase
	{
	}

	public interface IOptionalMechanicContext : IOptional, IBase
	{
	}

	public interface IOptionalRule : IOptional, IBase
	{
	}

	public interface IOptionalAbility : IOptional, IBase
	{
	}

	public interface IOptionalContextCaster : IOptional, IBase
	{
	}

	public interface IOptionalContextMainTargetEntity : IRequired, IBase
	{
	}

	public interface IOptionalContextMainTarget : IOptional, IBase
	{
	}

	public interface IOptionalCurrentTargetEntity : IOptional, IBase
	{
	}

	public interface IOptionalCurrentTarget : IOptional, IBase
	{
	}

	public interface IOptionalRuleInitiator : IOptional, IBase
	{
	}

	public interface IOptionalRuleTarget : IOptional, IBase
	{
	}

	public interface IOptionalAbilityWeapon : IOptional, IBase
	{
	}

	public interface IOptionalTargetByType : IOptional, IBase
	{
	}

	public interface IOptionalFact : IOptional, IBase
	{
	}

	private static PropertyContext? PropertyContext => ContextData<PropertyContextData>.Current?.Context;

	private static Exception BuildException<T>(this T _) where T : IBase
	{
		return new Exception("Failed to get property by _ " + typeof(T).Name);
	}

	[NotNull]
	public static MechanicsContext GetMechanicContext(this IMechanicContext accessor)
	{
		return PropertyContext?.MechanicContext ?? throw accessor.BuildException();
	}

	[NotNull]
	public static RulebookEvent GetRule(this IRule accessor)
	{
		return PropertyContext?.Rule ?? throw accessor.BuildException();
	}

	[NotNull]
	public static AbilityData GetAbility(this IAbility accessor)
	{
		return PropertyContext?.Ability ?? throw accessor.BuildException();
	}

	[NotNull]
	public static Entity GetContextCaster(this IContextCaster accessor)
	{
		return PropertyContext?.ContextCaster ?? throw accessor.BuildException();
	}

	[NotNull]
	public static MechanicEntity GetCurrentTarget(this ICurrentTargetEntity accessor)
	{
		return PropertyContext?.CurrentTarget ?? throw accessor.BuildException();
	}

	public static Vector3 GetCurrentTargetPosition(this ICurrentTarget accessor)
	{
		return PropertyContext?.ContextMainTargetPosition ?? throw accessor.BuildException();
	}

	public static float GetCurrentTargetOrientation(this ICurrentTarget accessor)
	{
		return PropertyContext?.ContextMainTargetOrientation ?? throw accessor.BuildException();
	}

	[NotNull]
	public static MechanicEntity GetContextMainTarget(this IContextMainTargetEntity accessor)
	{
		return PropertyContext?.ContextMainTarget ?? throw accessor.BuildException();
	}

	public static Vector3 GetContextMainTargetPosition(this IContextMainTarget accessor)
	{
		return PropertyContext?.CurrentTargetPosition ?? throw accessor.BuildException();
	}

	public static float GetContextMainTargetOrientation(this IContextMainTarget accessor)
	{
		return PropertyContext?.CurrentTargetOrientation ?? throw accessor.BuildException();
	}

	[NotNull]
	public static Entity GetRuleInitiator(this IRuleInitiator accessor)
	{
		return (PropertyContext?.Rule?.Initiator as MechanicEntity) ?? throw accessor.BuildException();
	}

	[NotNull]
	public static Entity GetRuleTarget(this IRuleTarget accessor)
	{
		return (PropertyContext?.Rule?.GetRuleTarget() as MechanicEntity) ?? throw accessor.BuildException();
	}

	[NotNull]
	public static ItemEntityWeapon GetAbilityWeapon(this IAbilityWeapon accessor)
	{
		return ((PropertyContext?.Rule is RuleCalculateStatsWeapon ruleCalculateStatsWeapon) ? ruleCalculateStatsWeapon.Weapon : PropertyContext?.Ability?.Weapon) ?? throw accessor.BuildException();
	}

	[NotNull]
	public static Entity GetTargetByType(this ITargetByType accessor, PropertyTargetType type)
	{
		return PropertyContext?.GetTargetEntity(type) ?? throw accessor.BuildException();
	}

	public static Vector3 GetTargetPositionByType(this ITargetByType accessor, PropertyTargetType type)
	{
		return PropertyContext?.GetTargetPosition(type) ?? throw accessor.BuildException();
	}

	public static IntRect GetTargetRectByType(this ITargetByType accessor, PropertyTargetType type)
	{
		return PropertyContext?.GetTargetRectByType(type) ?? throw accessor.BuildException();
	}

	public static BlueprintScriptableObject GetOwnerBlueprint(this IOwnerBlueprint accessor)
	{
		return (BlueprintScriptableObject)((Element)accessor).Owner;
	}

	[CanBeNull]
	public static MechanicsContext GetMechanicContext(this IOptionalMechanicContext _)
	{
		return PropertyContext?.MechanicContext;
	}

	[CanBeNull]
	public static RulebookEvent GetRule(this IOptionalRule _)
	{
		return PropertyContext?.Rule;
	}

	[CanBeNull]
	public static AbilityData GetAbility(this IOptionalAbility _)
	{
		return PropertyContext?.Ability;
	}

	[CanBeNull]
	public static Entity GetContextCaster(this IOptionalContextCaster _)
	{
		return PropertyContext?.ContextCaster;
	}

	[CanBeNull]
	public static MechanicEntity GetCurrentTarget(this IOptionalCurrentTargetEntity _)
	{
		return PropertyContext?.CurrentTarget;
	}

	[CanBeNull]
	public static Vector3? GetCurrentTargetPosition(this IOptionalCurrentTarget _)
	{
		return PropertyContext?.CurrentTargetPosition;
	}

	[CanBeNull]
	public static float? GetCurrentTargetOrientation(this IOptionalCurrentTarget _)
	{
		return PropertyContext?.CurrentTargetOrientation;
	}

	[CanBeNull]
	public static MechanicEntity GetContextMainTarget(this IOptionalContextMainTargetEntity _)
	{
		return PropertyContext?.ContextMainTarget;
	}

	[CanBeNull]
	public static Vector3? GetContextMainTargetPosition(this IOptionalContextMainTarget _)
	{
		return PropertyContext?.ContextMainTargetPosition;
	}

	[CanBeNull]
	public static float? GetContextMainTargetOrientation(this IOptionalContextMainTarget _)
	{
		return PropertyContext?.ContextMainTargetOrientation;
	}

	[CanBeNull]
	public static Entity GetRuleInitiator(this IOptionalRuleInitiator _)
	{
		return PropertyContext?.Rule?.Initiator as MechanicEntity;
	}

	[CanBeNull]
	public static Entity GetRuleTarget(this IOptionalRuleTarget _)
	{
		return PropertyContext?.Rule?.GetRuleTarget() as MechanicEntity;
	}

	[CanBeNull]
	public static ItemEntityWeapon GetAbilityWeapon(this IOptionalAbilityWeapon _)
	{
		if (!(PropertyContext?.Rule is RuleCalculateStatsWeapon ruleCalculateStatsWeapon))
		{
			return PropertyContext?.Ability?.Weapon;
		}
		return ruleCalculateStatsWeapon.Weapon;
	}

	[CanBeNull]
	public static Entity GetTargetByType(this IOptionalTargetByType _, PropertyTargetType type)
	{
		return PropertyContext?.GetTargetEntity(type);
	}

	[CanBeNull]
	public static Vector3? GetTargetPositionByType(this IOptionalTargetByType _, PropertyTargetType type)
	{
		return PropertyContext?.GetTargetPosition(type);
	}

	[CanBeNull]
	public static IntRect? GetTargetRectByType(this IOptionalTargetByType _, PropertyTargetType type)
	{
		return PropertyContext?.GetTargetRectByType(type);
	}

	[CanBeNull]
	public static MechanicEntityFact GetFact(this IOptionalFact _)
	{
		return PropertyContext?.Fact;
	}
}
