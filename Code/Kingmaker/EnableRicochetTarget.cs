using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker;

[TypeId("286d5faef53767c4a953caf3d08a5879")]
public class EnableRicochetTarget : EnableRicochet, IInitiatorRulebookHandler<RuleCalculateOverpenetration>, IRulebookHandler<RuleCalculateOverpenetration>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
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
