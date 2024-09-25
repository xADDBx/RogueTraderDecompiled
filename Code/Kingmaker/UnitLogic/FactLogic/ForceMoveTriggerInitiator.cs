using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("453af3fd1f6d418d85ace4f7e693b012")]
public class ForceMoveTriggerInitiator : ForceMoveTrigger, IInitiatorRulebookHandler<RulePerformAttack>, IRulebookHandler<RulePerformAttack>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ActionList Actions;

	protected override void OnTrigger(RulePerformAttack rule)
	{
		if (base.Fact.MaybeContext != null)
		{
			base.Fact.RunActionInContext(Actions, rule.ConcreteInitiator.ToITargetWrapper());
		}
		else
		{
			Actions.Run();
		}
	}

	public void OnEventAboutToTrigger(RulePerformAttack rule)
	{
	}

	public void OnEventDidTrigger(RulePerformAttack rule)
	{
		TryTrigger(rule);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
