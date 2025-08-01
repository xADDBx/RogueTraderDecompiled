using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowMultipleComponents]
[TypeId("850e481c33c718f419591b97dfed5f54")]
public class SavingThrowBonusAgainstAbilityType : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public AbilityType AbilityType;

	public ModifierDescriptor ModifierDescriptor;

	public int Value;

	public ContextValue Bonus;

	public bool OnlyPositiveValue;

	[Tooltip("Всегда успех")]
	public bool AlwaysSucceed;

	[Tooltip("Всегда провал")]
	public bool AlwaysFail;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		if (evt.Reason.Context == null || !(evt.Reason.Ability != null) || evt.Reason.Ability.Blueprint.Type != AbilityType)
		{
			return;
		}
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
		int num = Bonus.Calculate(base.Context) + Value * base.Fact.GetRank();
		if (!OnlyPositiveValue || num > Value)
		{
			evt.AddValueModifiers(num, base.Fact, ModifierDescriptor);
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
