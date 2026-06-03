using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Components;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("e0a93bfffabc89d4b8d333b6d315cbcf")]
public abstract class AllowRicochetWithoutHitting : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions;

	protected void ApplyToEvent(RuleCalculateOverpenetration rule)
	{
		PropertyContext context = new PropertyContext(rule.ConcreteInitiator, null, rule.MaybeTarget, null, rule, rule.Ability);
		if (m_Restrictions.IsPassed(context))
		{
			rule.AllowRicochetWithoutHitting = true;
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
