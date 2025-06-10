using Code.Enums;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[TypeId("88cbcee57286420e9d122e8588f3c111")]
public class SavingThrowBonusAgainstDoT : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformSavingThrow>, IRulebookHandler<RulePerformSavingThrow>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ModifierDescriptor ModifierDescriptor;

	public ContextValue Bonus;

	public DOT Type;

	[Tooltip("Всегда успех")]
	public bool AlwaysSucceed;

	[Tooltip("Всегда провал")]
	public bool AlwaysFail;

	public void OnEventAboutToTrigger(RulePerformSavingThrow evt)
	{
		if (!(evt.Reason.Fact is Buff buff))
		{
			return;
		}
		DOTLogic component = buff.GetComponent<DOTLogic>();
		if (component != null && component.Type == Type)
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
			int value = Bonus.Calculate(base.Context);
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
