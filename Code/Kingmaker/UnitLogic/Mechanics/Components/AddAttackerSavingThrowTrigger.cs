using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[AllowMultipleComponents]
[TypeId("aa361ba866af4258ade424a4951bae3f")]
public class AddAttackerSavingThrowTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool OnlyPass;

	public bool OnlyFail;

	public ActionList Action;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if ((evt.Reason.Ability?.Caster == base.Owner || evt.Reason.Caster == base.Owner) && CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, evt.InitiatorUnit.ToITargetWrapper());
		}
	}

	private bool CheckConditions(RulePerformSavingThrow evt)
	{
		if (OnlyPass && !evt.IsPassed)
		{
			return false;
		}
		if (OnlyFail && evt.IsPassed)
		{
			return false;
		}
		if (!Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability))
		{
			return false;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
