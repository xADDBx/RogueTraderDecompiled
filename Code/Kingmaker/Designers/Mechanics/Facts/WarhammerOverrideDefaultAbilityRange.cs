using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("62914d9ab9fe4dada246891867955ddd")]
public class WarhammerOverrideDefaultAbilityRange : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateAbilityRange>, IRulebookHandler<RuleCalculateAbilityRange>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private PropertyCalculator m_Range;

	public void OnEventAboutToTrigger(RuleCalculateAbilityRange evt)
	{
		if (m_Restrictions.IsPassed(base.Fact, evt, evt.Ability) && evt.Ability.Blueprint == base.OwnerBlueprint)
		{
			if (m_Range == null)
			{
				PFLog.Default.Error("Range calculator is missing");
				return;
			}
			MechanicsContext mechanicContext = new MechanicsContext(evt.ConcreteInitiator, evt.ConcreteInitiator, base.Fact.Blueprint);
			evt.OverrideRange = m_Range.GetValue(new PropertyContext(evt.ConcreteInitiator, base.Fact, evt.ConcreteInitiator, mechanicContext, evt, evt.Ability));
		}
	}

	public void OnEventDidTrigger(RuleCalculateAbilityRange evt)
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
