using System;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;

namespace Kingmaker.Controllers;

public class OffensiveActionsController : IController, IApplyAbilityEffectHandler, ISubscriber, IAreaEffectEnterHandler, ISubscriber<IAreaEffectEntity>, IGlobalRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, IGlobalRulebookSubscriber
{
	private static bool IsFake(MechanicEntity entity)
	{
		if (entity is UnitEntity unitEntity)
		{
			return unitEntity.Blueprint.IsFake;
		}
		return false;
	}

	private static bool IsSuitableEntity(MechanicEntity entity)
	{
		if (entity.IsInState && entity.IsInGame && !IsFake(entity))
		{
			return !entity.IsDisposed;
		}
		return false;
	}

	private static void RaiseEvents(MechanicEntity initiator, MechanicEntity target)
	{
		if (!(initiator is BaseUnitEntity entity))
		{
			return;
		}
		BaseUnitEntity targetUnit = target as BaseUnitEntity;
		if (targetUnit != null && IsSuitableEntity(initiator) && IsSuitableEntity(target))
		{
			EventBus.RaiseEvent((IBaseUnitEntity)entity, (Action<IUnitMakeOffensiveActionHandler>)delegate(IUnitMakeOffensiveActionHandler h)
			{
				h.HandleUnitMakeOffensiveAction(targetUnit);
			}, isCheckRuntime: true);
		}
	}

	public void OnAbilityEffectApplied(AbilityExecutionContext context)
	{
	}

	public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
		if (!context.AbilityBlueprint.NotOffensive && target.Target.Entity != null && (context.Caster.IsEnemy(target.Target.Entity) || target.Target.Entity.IsNeutral))
		{
			RaiseEvents(context.Caster, target.Target.Entity);
		}
	}

	public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
	{
	}

	public void HandleUnitEnterAreaEffect(BaseUnitEntity unit)
	{
		AreaEffectEntity entity = EventInvokerExtensions.GetEntity<AreaEffectEntity>();
		MechanicEntity maybeCaster = entity.Context.MaybeCaster;
		if (maybeCaster is BaseUnitEntity initiator && unit.CombatGroup.IsEnemy(maybeCaster) && entity.AffectEnemies && entity.AggroEnemies)
		{
			RaiseEvents(initiator, unit);
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		AbilityData ability = evt.Ability;
		if (((object)ability == null || !ability.Blueprint.NotOffensive) && (evt.Initiator.IsEnemy(evt.Target) || evt.Target.IsNeutral))
		{
			RaiseEvents((MechanicEntity)evt.Initiator, (MechanicEntity)evt.Target);
		}
	}
}
