using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker;

[TypeId("47795bd445fea2f429edd75d0d0a11a1")]
public class EnableRicochetInitiator : EnableRicochet, IInitiatorRulebookHandler<RuleCalculateOverpenetration>, IRulebookHandler<RuleCalculateOverpenetration>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleCalculateOverpenetration evt)
	{
		ApplyToEvent(evt);
	}

	public void OnEventDidTrigger(RuleCalculateOverpenetration evt)
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
