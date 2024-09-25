using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.EventConditionActionSystem.Events;

[TypeId("b81fe82d8ccd42b3b2864f38c833ccfa")]
public abstract class WarhammerDefenseTriggerBase : MechanicEntityFactComponentDelegate, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList ActionOnSelfHit;

	public ActionList ActionOnSelfMiss;

	public ActionList ActionsOnTargetHit;

	public ActionList ActionsOnTargetMiss;

	public bool TriggerOnDodge;

	public bool TriggerOnParry;

	public bool TriggerOnCover;

	protected void TryTriggerDodgeActions(RulePerformDodge evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		if (TriggerOnDodge)
		{
			if (!evt.Result)
			{
				base.Fact.RunActionInContext(ActionOnSelfHit, (TargetWrapper)evt.Attacker);
				base.Fact.RunActionInContext(ActionsOnTargetHit, (TargetWrapper)evt.Defender);
			}
			if (evt.Result)
			{
				base.Fact.RunActionInContext(ActionOnSelfMiss, (TargetWrapper)evt.Attacker);
				base.Fact.RunActionInContext(ActionsOnTargetMiss, (TargetWrapper)evt.Defender);
			}
			base.ExecutesCount++;
		}
	}

	protected void TryTriggerParryActions(RuleRollParry evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Ability))
			{
				return;
			}
		}
		if (TriggerOnParry)
		{
			if (!evt.Result)
			{
				base.Fact.RunActionInContext(ActionOnSelfHit, (TargetWrapper)evt.Attacker);
				base.Fact.RunActionInContext(ActionsOnTargetHit, (TargetWrapper)evt.Defender);
			}
			if (evt.Result)
			{
				base.Fact.RunActionInContext(ActionOnSelfMiss, (TargetWrapper)evt.Attacker);
				base.Fact.RunActionInContext(ActionsOnTargetMiss, (TargetWrapper)evt.Defender);
			}
			base.ExecutesCount++;
		}
	}

	protected void TryTriggerCoverActions(RuleRollCoverHit evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability))
			{
				return;
			}
		}
		if (TriggerOnCover)
		{
			if (!evt.ResultIsHit)
			{
				base.Fact.RunActionInContext(ActionOnSelfHit, (TargetWrapper)evt.ConcreteInitiator);
				base.Fact.RunActionInContext(ActionsOnTargetHit, (TargetWrapper)evt.MaybeTarget);
			}
			if (evt.ResultIsHit)
			{
				base.Fact.RunActionInContext(ActionOnSelfMiss, (TargetWrapper)evt.ConcreteInitiator);
				base.Fact.RunActionInContext(ActionsOnTargetMiss, (TargetWrapper)evt.MaybeTarget);
			}
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
