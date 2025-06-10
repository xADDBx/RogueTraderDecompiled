using System;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Sound.Base;
using Kingmaker.UI.Models.UnitSettings;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Visual.Sound;

public class CastAsksController : IUnitAsksController, IDisposable, IClickActionHandler, ISubscriber, IRulebookHandler<RuleHealDamage>, IRulebookHandler<RulePerformMedicaeHeal>, IClickMechanicActionBarSlotHandler
{
	public CastAsksController()
	{
		EventBus.Subscribe(this);
	}

	void IDisposable.Dispose()
	{
		EventBus.Unsubscribe(this);
	}

	void IClickActionHandler.OnAbilityCastRefused(AbilityData ability, TargetWrapper target, IAbilityTargetRestriction failedRestriction)
	{
		ability.Caster?.View.Asks?.CantDo.Schedule();
	}

	void IClickActionHandler.OnMoveRequested(Vector3 target)
	{
	}

	void IClickActionHandler.OnCastRequested(AbilityData ability, TargetWrapper target)
	{
		ScheduleOrder(ability, target);
	}

	void IClickActionHandler.OnItemUseRequested(AbilityData ability, TargetWrapper target)
	{
		ScheduleOrder(ability, target);
	}

	void IClickActionHandler.OnAttackRequested(BaseUnitEntity unit, UnitEntityView target)
	{
	}

	void IRulebookHandler<RuleHealDamage>.OnEventAboutToTrigger(RuleHealDamage evt)
	{
		ScheduleHeal(evt.ConcreteInitiator, evt.ConcreteTarget);
	}

	void IRulebookHandler<RuleHealDamage>.OnEventDidTrigger(RuleHealDamage evt)
	{
	}

	void IRulebookHandler<RulePerformMedicaeHeal>.OnEventAboutToTrigger(RulePerformMedicaeHeal evt)
	{
		ScheduleHeal(evt.ConcreteInitiator, evt.ConcreteTarget);
	}

	void IRulebookHandler<RulePerformMedicaeHeal>.OnEventDidTrigger(RulePerformMedicaeHeal evt)
	{
	}

	private static void ScheduleHeal(MechanicEntity initiator, MechanicEntity target)
	{
		if (initiator != null && target != null)
		{
			if (initiator == target)
			{
				initiator.View.Asks?.BeingHealed.Schedule();
			}
			else if (target.IsAlly(initiator) && (!(initiator.View != null) || initiator.View.Asks == null || !initiator.View.Asks.HealingAlly.Schedule(is2D: false, delegate
			{
				HealAllyCallback(target);
			})))
			{
				target.View.Asks?.BeingHealed.Schedule();
			}
		}
	}

	private static void HealAllyCallback(MechanicEntity target)
	{
		if (target != null && !(target.View == null) && target.View.Asks != null)
		{
			PartLifeState lifeStateOptional = target.GetLifeStateOptional();
			if (lifeStateOptional != null && lifeStateOptional.IsConscious)
			{
				target.View.Asks?.BeingHealed.Schedule();
			}
		}
	}

	void IClickMechanicActionBarSlotHandler.HandleClickMechanicActionBarSlot(MechanicActionBarSlot action)
	{
		if (action.IsPossibleActive || action.Unit == null || action.Unit.View == null || action.Unit.View.Asks == null)
		{
			return;
		}
		if (action is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && !mechanicActionBarSlotAbility.Ability.HasEnoughAmmo && mechanicActionBarSlotAbility.Ability?.Weapon?.Blueprint.VisualParameters.OutOfAmmoSound != null)
		{
			SoundEventsManager.PostEvent(mechanicActionBarSlotAbility.Ability.Weapon.Blueprint.VisualParameters.OutOfAmmoSound, mechanicActionBarSlotAbility.Unit.View.gameObject);
		}
		if (!(action is MechanicActionBarSlotEmpty))
		{
			if (action is MechanicActionBarSlotAbility mechanicActionBarSlotAbility2 && !mechanicActionBarSlotAbility2.Ability.HasEnoughAmmo && action.Unit.View.Asks.OutOfAmmo.HasBarks)
			{
				action.Unit.View.Asks.OutOfAmmo.Schedule();
			}
			else
			{
				action.Unit.View.Asks.CantDo.Schedule();
			}
		}
	}

	private static void ScheduleOrder(AbilityData ability, TargetWrapper target)
	{
		if (ability.Caster == null || ability.Caster.View == null || ability.Caster.View.Asks == null)
		{
			return;
		}
		using (ContextData<MechanicsContext.Data>.Request().Setup(new MechanicsContext(ability.Caster, ability.Caster, ability.Caster.Blueprint), ability.Caster))
		{
			using (ContextData<PropertyContextData>.Request().Setup(new PropertyContext(ability, target.Entity)))
			{
				ContextData<MechanicsContext.Data>.Current.Context.SetSourceAbility(ability.Blueprint);
				if (ability.Blueprint.IsMomentum && ability.Caster.View.Asks.MomentumAction.HasBarks)
				{
					ability.Caster.View.Asks.MomentumAction.Schedule();
					return;
				}
				BarkWrapper barkWrapper = ability.Caster.View.Asks.Order;
				switch (ability.Blueprint.AbilityTag)
				{
				case AbilityTag.ThrowingGrenade:
					if (ability.Caster.View.Asks.ThrowingGrenade.HasBarks)
					{
						barkWrapper = ability.Caster.View.Asks.ThrowingGrenade;
					}
					break;
				case AbilityTag.UsingCombatDrug:
					if (ability.Caster.View.Asks.UsingCombatDrug.HasBarks)
					{
						barkWrapper = ability.Caster.View.Asks.UsingCombatDrug;
					}
					break;
				case AbilityTag.Heal:
					ScheduleHeal(ability.Caster, target.Entity);
					return;
				case AbilityTag.StarshipShotAbility:
				case AbilityTag.StarshipUltimateAbility:
					return;
				default:
					throw new ArgumentOutOfRangeException();
				case AbilityTag.None:
					break;
				}
				barkWrapper?.Schedule();
			}
		}
	}
}
