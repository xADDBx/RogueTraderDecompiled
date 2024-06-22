using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("e45eddecc6b973948aa0f785b8954827")]
public abstract class ContextAction : GameAction
{
	[CanBeNull]
	protected AbilityExecutionContext AbilityContext => Context as AbilityExecutionContext;

	protected MechanicsContext Context => ContextData<MechanicsContext.Data>.Current?.Context;

	[NotNull]
	protected MechanicEntity Caster => Context.MaybeCaster ?? throw new Exception("Caster is missing");

	protected TargetWrapper Target => ContextData<MechanicsContext.Data>.Current?.CurrentTarget;

	[NotNull]
	protected MechanicEntity TargetEntity => Target.Entity ?? throw new Exception("Target entity is missing");

	[CanBeNull]
	protected Projectile Projectile => ContextData<AbilityExecutionContext.Data>.Current?.Projectile;

	[CanBeNull]
	protected RulePerformAttack AttackRule => ContextData<AbilityExecutionContext.Data>.Current?.AttackRule;

	public virtual bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition)
	{
		return true;
	}
}
