using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View.Covers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dcf580788bcc4b01be0853ad7f2b44cd")]
public class FakeCover : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateHitChances>, IRulebookHandler<RuleCalculateHitChances>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[SerializeField]
	private LosCalculations.CoverType m_CoverType;

	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private bool m_IgnoreRealCover;

	public void OnEventAboutToTrigger(RuleCalculateHitChances evt)
	{
		if (m_Restrictions.IsPassed(base.Fact, evt, evt.Ability))
		{
			evt.SetFakeCover(m_CoverType, m_IgnoreRealCover);
		}
	}

	public void OnEventDidTrigger(RuleCalculateHitChances evt)
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
