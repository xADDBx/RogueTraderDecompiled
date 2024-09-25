using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("88936dcd2e649e6418757b6b0595d16a")]
public class AddImmunityToEnergyDrain : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDrainEnergy>, IRulebookHandler<RuleDrainEnergy>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public void OnEventAboutToTrigger(RuleDrainEnergy evt)
	{
		evt.TargetIsImmune = true;
	}

	public void OnEventDidTrigger(RuleDrainEnergy evt)
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
