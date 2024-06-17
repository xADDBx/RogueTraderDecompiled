using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[TypeId("c6cf387a8f814804b91629d47fc919e0")]
public class PushCollisionDamageBonus : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformCollision>, IRulebookHandler<RulePerformCollision>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	public ContextValueModifierWithType Bonus = new ContextValueModifierWithType();

	public ModifierDescriptor Descriptor;

	public void OnEventAboutToTrigger(RulePerformCollision evt)
	{
		if (m_Restrictions.IsPassed(base.Fact, evt))
		{
			Bonus.TryApply(evt.DamageModifiers, base.Fact, Descriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformCollision evt)
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
