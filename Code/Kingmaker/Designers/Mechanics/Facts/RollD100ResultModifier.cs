using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("908974db80a647d4b39251b549586e8c")]
public class RollD100ResultModifier : MechanicEntityFactComponentDelegate, IGlobalRulebookHandler<RuleRollD100>, IRulebookHandler<RuleRollD100>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private bool UseModifierAsRollResult;

	[SerializeField]
	private PropertyCalculator m_Modifier = new PropertyCalculator();

	public void OnEventAboutToTrigger(RuleRollD100 evt)
	{
	}

	public void OnEventDidTrigger(RuleRollD100 evt)
	{
		if (evt.Initiator != base.Owner || !m_Restrictions.IsPassed(base.Fact, evt))
		{
			return;
		}
		if (m_Modifier == null)
		{
			PFLog.Default.Error("Result bonus calculator is missing");
			return;
		}
		int value = m_Modifier.GetValue(new PropertyContext(evt.ConcreteInitiator, base.Fact, evt.ConcreteInitiator, null, evt));
		if (UseModifierAsRollResult)
		{
			evt.Override(value);
		}
		else
		{
			evt.AddModifier(value);
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
