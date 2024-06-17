using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b28e1f5ac7e12074e9457c0f862f1ae8")]
public class WarhammerIncomingDamageNullifier : UnitFactComponentDelegate, ITargetRulebookHandler<RuleRollDamage>, IRulebookHandler<RuleRollDamage>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ContextValue m_NullifyChances;

	[SerializeField]
	private bool m_OnlyCritical;

	private int NullifyChances => Math.Max(Math.Min(m_NullifyChances.Calculate(base.Context), 100), 0);

	public void OnEventAboutToTrigger(RuleRollDamage evt)
	{
		using (ContextData<SavableTriggerData>.Request().Setup(base.ExecutesCount))
		{
			if (!Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability))
			{
				return;
			}
		}
		if (!m_OnlyCritical || evt.Damage.IsCritical)
		{
			evt.NullifyInformation.AddNullifyBuff(base.Fact as Buff);
			evt.NullifyInformation.DamageChance = (evt.NullifyInformation.HasDamageChance ? (evt.NullifyInformation.DamageChance * (100 + NullifyChances) / 100) : NullifyChances);
		}
	}

	public void OnEventDidTrigger(RuleRollDamage evt)
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
