using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;

namespace Kingmaker.UI.Models.Log.Events;

public class GameLogEventImmuneToAbility : GameLogEvent<GameLogEventImmuneToAbility>
{
	private class EventsHandler : GameLogController.GameEventsHandler, IUIContextActionRunHandler, ISubscriber, IApplyAbilityEffectHandler, IDeliverAbilityEffectHandler
	{
		private void TryAddEvent(BlueprintAbility ability, MechanicEntity target)
		{
			if (ability != null && target is IAbstractUnitEntity target2 && target.HasAbilityImmunity(ability))
			{
				AddEvent(new GameLogEventImmuneToAbility(ability, target2));
			}
		}

		public void HandleOnContextActionRun(MechanicsContext context, MechanicEntity caster, MechanicEntity target)
		{
			TryAddEvent(context.SourceAbility, target);
		}

		public void OnAbilityEffectApplied(AbilityExecutionContext context)
		{
			TryAddEvent(context.AbilityBlueprint, context.MainTarget?.Entity);
		}

		public void OnAbilityEffectAppliedToTarget(AbilityExecutionContext context, AbilityDeliveryTarget target)
		{
			TryAddEvent(context.AbilityBlueprint, target?.Target?.Entity);
		}

		public void OnDeliverAbilityEffect(AbilityExecutionContext context, TargetWrapper target)
		{
			TryAddEvent(context.AbilityBlueprint, target?.Entity);
		}

		public void OnTryToApplyAbilityEffect(AbilityExecutionContext context, AbilityDeliveryTarget target)
		{
			TryAddEvent(context.AbilityBlueprint, target?.Target?.Entity);
		}
	}

	public readonly UnitReference Target;

	public readonly BlueprintAbility Ability;

	private GameLogEventImmuneToAbility(BlueprintAbility ability, IAbstractUnitEntity target)
	{
		Target = UnitReference.FromIAbstractUnitEntity(target);
		Ability = ability;
	}

	protected override bool TrySwallowEventInternal(GameLogEvent @event)
	{
		if (@event is GameLogEventImmuneToAbility gameLogEventImmuneToAbility)
		{
			if (Target == gameLogEventImmuneToAbility.Target && Ability == gameLogEventImmuneToAbility.Ability)
			{
				return true;
			}
			return base.TrySwallowEventInternal(@event);
		}
		return base.TrySwallowEventInternal(@event);
	}
}
