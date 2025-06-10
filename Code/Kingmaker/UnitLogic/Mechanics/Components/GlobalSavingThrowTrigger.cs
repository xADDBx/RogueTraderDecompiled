using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[AllowMultipleComponents]
[TypeId("c99a7aebae0d4b05846be5c25338fc86")]
public class GlobalSavingThrowTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool OnlyPass;

	public bool OnlyFail;

	public SavingThrowType SavingThrowType;

	public ActionList Action;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
	{
		if ((SavingThrowType == SavingThrowType.Unknown || SavingThrowType == evt.Type) && CheckConditions(evt) && base.Fact.MaybeContext != null)
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
