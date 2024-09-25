using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("cf847c0bb06a49aa917e80795d2d8436")]
public class GlobalMomentumGainFlatBonus : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformMomentumChange>, IRulebookHandler<RulePerformMomentumChange>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public int Bonus;

	public void OnEventAboutToTrigger(RulePerformMomentumChange evt)
	{
		if ((!base.Owner.IsPlayerFaction || base.Owner.IsInPlayerParty) && base.Owner.IsInCombat)
		{
			evt.FlatBonus += Bonus;
		}
	}

	public void OnEventDidTrigger(RulePerformMomentumChange evt)
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
