using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Block;
using Kingmaker.UnitLogic.Abilities;
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

	public bool TriggerOnBlock;

	protected void TryTriggerDodgeActions(RulePerformDodge evt)
	{
		if (TriggerOnDodge && CheckRestrictions(evt, evt.Ability))
		{
			RunActions(!evt.Result, evt.Attacker, evt.Defender);
			base.ExecutesCount++;
		}
	}

	protected void TryTriggerParryActions(RuleRollParry evt)
	{
		if (TriggerOnParry && CheckRestrictions(evt, evt.Ability))
		{
			RunActions(!evt.Result, evt.Attacker, evt.Defender);
			base.ExecutesCount++;
		}
	}

	protected void TryTriggerBlockActions(RuleRollBlock evt)
	{
		if (TriggerOnBlock && CheckRestrictions(evt, evt.ChancesRule.Ability))
		{
			RunActions(!evt.Result, evt.Attacker, evt.Defender);
			base.ExecutesCount++;
		}
	}

	protected void TryTriggerCoverActions(RuleRollCoverHit evt)
	{
		if (TriggerOnCover && CheckRestrictions(evt, evt.Reason.Ability))
		{
			RunActions(!evt.ResultIsHit, evt.ConcreteInitiator, evt.MaybeTarget);
			base.ExecutesCount++;
		}
	}

	private bool CheckRestrictions(RulebookEvent evt, AbilityData ability)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, ability))
			{
				return false;
			}
		}
		return true;
	}

	private void RunActions(bool isHit, MechanicEntity self, MechanicEntity target)
	{
		if (isHit)
		{
			base.Fact.RunActionInContext(ActionOnSelfHit, (TargetWrapper)self);
			base.Fact.RunActionInContext(ActionsOnTargetHit, (TargetWrapper)target);
		}
		else
		{
			base.Fact.RunActionInContext(ActionOnSelfMiss, (TargetWrapper)self);
			base.Fact.RunActionInContext(ActionsOnTargetMiss, (TargetWrapper)target);
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
