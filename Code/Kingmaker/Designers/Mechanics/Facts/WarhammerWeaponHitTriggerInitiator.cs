using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("67bd9d5235e74484bb1d673b885ab430")]
public class WarhammerWeaponHitTriggerInitiator : WarhammerWeaponHitTriggerBase, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, IFakeCriticalHandler<EntitySubscriber>, IFakeCriticalHandler, ISubscriber<IBaseUnitEntity>, IEventTag<IFakeCriticalHandler, EntitySubscriber>, IHashable
{
	public bool OnlyRighteousFury;

	public bool OnlyMelee;

	public void OnEventAboutToTrigger(RulePerformAttack evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		CheckConditionsAndRunActions(evt.ConcreteInitiator, evt.ConcreteTarget, evt.RollPerformAttackRule.IsMelee, evt.RollPerformAttackRule.ResultIsRighteousFury, evt.ResultIsHit);
	}

	private void CheckConditionsAndRunActions(MechanicEntity initiator, MechanicEntity target, bool isMelee, bool isCritical, bool isHit)
	{
		if ((!OnlyRighteousFury || isCritical) && (!OnlyMelee || isMelee))
		{
			TryRunActions(initiator, target, isHit);
		}
	}

	public void HandleFakeCritical(RuleDealDamage evt)
	{
		BaseUnitEntity targetUnit = evt.TargetUnit;
		if (targetUnit == null || !OnlyRighteousFury)
		{
			return;
		}
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.SourceAbility))
			{
				return;
			}
		}
		CheckConditionsAndRunActions(evt.ConcreteInitiator, targetUnit, evt.SourceAbility?.IsMelee ?? false, isCritical: true, isHit: true);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
