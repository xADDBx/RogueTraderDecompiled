using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c19cadfe75cd6fc44b3f50e7fa124a26")]
public class WarhammerAfterAttackRollTriggerTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public class SavableData : IEntityFactComponentSavableData, IHashable
	{
		[JsonProperty]
		public TimeSpan TriggeredOnAbilityWithCastTime;

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			result.Append(ref TriggeredOnAbilityWithCastTime);
			return result;
		}
	}

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList Actions;

	public bool triggerAfterAttack;

	public bool onlyMeleeAttack;

	public bool ActionsOnTarget;

	public bool OnlyHit;

	[Tooltip("Only affects behaviour for abilities that trigger multiple RulePerformAttack rolls, like BurstFireBy default this component would trigger on every RulePerformAttack roll,but if this flag is set to true, the component will execute it actions only once per casted ability.")]
	public bool TriggerOncePerAbilityCast;

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
		TryToTrigger(evt, !triggerAfterAttack);
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		TryToTrigger(evt, triggerAfterAttack);
	}

	private void TryToTrigger(RulePerformAttack evt, bool afterAttackTrigger)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		if ((onlyMeleeAttack && !evt.IsMelee) || !afterAttackTrigger || (OnlyHit && !evt.ResultIsHit))
		{
			return;
		}
		if (TriggerOncePerAbilityCast)
		{
			TimeSpan? timeSpan = (evt.Reason.Context as AbilityExecutionContext)?.CastTime;
			if (timeSpan.HasValue)
			{
				TimeSpan triggeredOnAbilityWithCastTime = RequestSavableData<SavableData>().TriggeredOnAbilityWithCastTime;
				TimeSpan? timeSpan2 = timeSpan;
				if (!(triggeredOnAbilityWithCastTime != timeSpan2))
				{
					return;
				}
				RequestSavableData<SavableData>().TriggeredOnAbilityWithCastTime = timeSpan.Value;
			}
		}
		base.Fact.RunActionInContext(Actions, (!ActionsOnTarget) ? evt.ConcreteInitiator.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
		base.ExecutesCount++;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
