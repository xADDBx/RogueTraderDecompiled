using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("35018634ecb04c539db2c88a5cd2ab44")]
public class RandomizeScatterShotDirections : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateScatterShotHitDirectionProbability>, IRulebookHandler<RuleCalculateScatterShotHitDirectionProbability>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restriction = new RestrictionCalculator();

	public ContextValue Main = new ContextValue
	{
		Value = 20
	};

	public ContextValue Near = new ContextValue
	{
		Value = 40
	};

	public ContextValue Far = new ContextValue
	{
		Value = 40
	};

	public void OnEventAboutToTrigger(RuleCalculateScatterShotHitDirectionProbability evt)
	{
		if (Restriction.IsPassed(base.Fact, evt, evt.Ability))
		{
			evt.OverrideResult(Main.Calculate(base.Context), Near.Calculate(base.Context), Far.Calculate(base.Context));
		}
	}

	public void OnEventDidTrigger(RuleCalculateScatterShotHitDirectionProbability evt)
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
