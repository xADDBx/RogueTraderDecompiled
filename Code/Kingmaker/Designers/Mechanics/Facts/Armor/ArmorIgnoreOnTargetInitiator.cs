using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Armor;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("b2e83cd85780453f854ebe9dac137c42")]
public class ArmorIgnoreOnTargetInitiator : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleRollDamage>, IRulebookHandler<RuleRollDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	private bool IgnoreDeflection;

	[SerializeField]
	private bool IgnoreArmourAbsorption;

	public void OnEventAboutToTrigger(RuleRollDamage evt)
	{
		if (Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability) && evt.ConcreteInitiator.IsEnemy(evt.ConcreteTarget))
		{
			if (IgnoreDeflection)
			{
				evt.IgnoreDeflection = true;
			}
			if (IgnoreArmourAbsorption)
			{
				evt.IgnoreArmourAbsorption = true;
			}
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
