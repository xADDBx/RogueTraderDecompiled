using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c19cadfe75cd6fc44b3f50e7fa124a26")]
public class WarhammerAfterAttackRollTriggerTarget : UnitFactComponentDelegate, ITargetRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList Actions;

	public bool triggerAfterAttack;

	public bool onlyMeleeAttack;

	public bool ActionsOnTarget;

	public bool OnlyHit;

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
		if ((!onlyMeleeAttack || evt.IsMelee) && afterAttackTrigger && (!OnlyHit || evt.ResultIsHit))
		{
			base.Fact.RunActionInContext(Actions, (!ActionsOnTarget) ? evt.ConcreteInitiator.ToITargetWrapper() : evt.ConcreteTarget.ToITargetWrapper());
			base.ExecutesCount++;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
