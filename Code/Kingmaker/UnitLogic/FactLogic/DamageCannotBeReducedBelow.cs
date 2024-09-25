using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Mechanics.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("43b9759cb68c49349492cdde1e29ec7b")]
public class DamageCannotBeReducedBelow : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	private enum ValueType
	{
		AsIs,
		PercentOfMaxDamage
	}

	private class Scope : ContextFlag<Scope>
	{
	}

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	[SerializeField]
	private ValueType m_ValueType;

	public PropertyCalculator Value;

	public void OnEventAboutToTrigger(RuleDealDamage evt)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if ((bool)ContextData<Scope>.Current)
		{
			return;
		}
		using (ContextData<Scope>.Request())
		{
			if (Restriction.IsPassed(base.Fact, base.Context, evt, evt.SourceAbility))
			{
				PropertyContext context = new PropertyContext(base.Owner, base.Fact, null, base.Context, evt, evt.SourceAbility);
				int num = ((m_ValueType == ValueType.PercentOfMaxDamage) ? Mathf.RoundToInt((float)Value.GetValue(context) / 100f * (float)evt.Damage.GetMaxValueWithoutPenalties()) : Value.GetValue(context));
				int num2 = Math.Max(0, num - evt.Result);
				if (num2 > 0)
				{
					DamageData damage = DamageType.Direct.CreateDamage(num2);
					Rulebook.Trigger(new RuleDealDamage(base.Owner, (MechanicEntity)evt.Target, damage));
				}
			}
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
