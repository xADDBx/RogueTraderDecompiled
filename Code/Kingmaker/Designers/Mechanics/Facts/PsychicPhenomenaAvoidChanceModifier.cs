using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("9d8e2e5df59c4268a1c54ee9b08c41a7")]
public class PsychicPhenomenaAvoidChanceModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, IRulebookHandler<RuleCalculatePsychicPhenomenaEffect>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue PsychicPhenomenaAvoidChance;

	public ContextValue PerilsOfTheWarpAvoidChance;

	public void OnEventAboutToTrigger(RuleCalculatePsychicPhenomenaEffect evt)
	{
		evt.PsychicPhenomenaAvoid.ChanceModifiers.Add(PsychicPhenomenaAvoidChance.Calculate(base.Context), base.Fact);
		evt.PerilsOfTheWarpAvoid.ChanceModifiers.Add(PerilsOfTheWarpAvoidChance.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculatePsychicPhenomenaEffect evt)
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
