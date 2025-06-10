using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
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

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("bcb56aa89bbc4381a0f7b268b6fc878e")]
public class SavingThrowBonusAgainstOwner : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ModifierDescriptor ModifierDescriptor;

	public int Value;

	public ContextValue Bonus;

	[Tooltip("Всегда успех")]
	public bool AlwaysSucceed;

	[Tooltip("Всегда провал")]
	public bool AlwaysFail;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		if (evt.Reason.Ability?.Caster == base.Owner && Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability) && evt.Reason.Context != null && evt.Reason.Ability != null)
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
			int value = Bonus.Calculate(base.Context) + Value * base.Fact.GetRank();
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
