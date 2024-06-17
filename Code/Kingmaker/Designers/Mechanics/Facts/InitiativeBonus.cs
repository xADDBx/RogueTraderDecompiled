using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("a4fd87c241554f3f8e5e1a10af29b5e2")]
public class InitiativeBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleRollInitiative>, IRulebookHandler<RuleRollInitiative>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue Value = 0;

	public void OnEventAboutToTrigger(RuleRollInitiative evt)
	{
		evt.Modifiers.Add(Value.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleRollInitiative evt)
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
