using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("10f4b27dd2dc44a1ad73fdedc3c7b6bf")]
public class SavingThrowBonusWithRestrictions : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ModifierDescriptor ModifierDescriptor;

	public int Multiplier = 1;

	public ContextValue Bonus;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public SavingThrowType Type;

	[Tooltip("Всегда успех")]
	public bool AlwaysSucceed;

	[Tooltip("Всегда провал")]
	public bool AlwaysFail;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		if ((Type == SavingThrowType.Unknown || evt.Type == Type) && Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability))
		{
			if (AlwaysSucceed)
			{
				evt.SetAlwaysSucceed(base.Fact, ModifierDescriptor);
				return;
			}
			if (AlwaysFail)
			{
				evt.SetAlwaysFail(base.Fact, ModifierDescriptor);
				return;
			}
			int value = Bonus.Calculate(base.Context) * Multiplier;
			evt.AddValueModifiers(value, base.Fact, ModifierDescriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformSavingThrow evt)
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
