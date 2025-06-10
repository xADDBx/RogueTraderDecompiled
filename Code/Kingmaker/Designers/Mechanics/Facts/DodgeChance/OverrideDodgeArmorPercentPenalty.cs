using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[TypeId("12333dde2cc34ec5b16355e0fbbece35")]
public class OverrideDodgeArmorPercentPenalty : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	[Range(-1f, 100f)]
	private int m_DodgeArmorPercentPenalty = 25;

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		evt.SetOverrideDodgeArmorPercentPenalty(m_DodgeArmorPercentPenalty);
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
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
