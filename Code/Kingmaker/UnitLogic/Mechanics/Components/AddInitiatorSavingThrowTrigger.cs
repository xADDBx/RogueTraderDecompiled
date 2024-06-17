using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[TypeId("2b7e896fea233934d9e41502f583205c")]
public class AddInitiatorSavingThrowTrigger : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
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
		if (CheckConditions(evt) && base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Action, base.OwnerTargetWrapper);
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
