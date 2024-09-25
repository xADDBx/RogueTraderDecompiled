using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
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
[TypeId("36e0e76d6e174ef28bd66ade47a316ac")]
public class SavingThrowBonusAgainstBuffCaster : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ModifierDescriptor ModifierDescriptor;

	public int Multiplier = 1;

	public ContextValue Bonus;

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public SavingThrowType Type;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		MechanicEntity mechanicEntity = evt.Reason.Ability?.Caster;
		if (mechanicEntity != null && mechanicEntity == base.Context.MaybeCaster && (Type == SavingThrowType.Unknown || evt.Type == Type) && Restrictions.IsPassed(base.Fact, evt, evt.Reason.Ability))
		{
			int value = Bonus.Calculate(base.Context) * Multiplier;
			evt.ValueModifiers.Add(value, base.Fact, ModifierDescriptor);
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
