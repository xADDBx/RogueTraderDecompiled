using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("858964321f47405180ca58a066db98f5")]
public class MomentumChangesModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RulePerformMomentumChange>, IRulebookHandler<RulePerformMomentumChange>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	private enum ChangeType
	{
		Positive,
		Negative
	}

	[SerializeField]
	private ChangeType m_ChangeType;

	public ModifierDescriptor ModifierDescriptor;

	public ContextValueModifierWithType Modifier;

	public void OnEventAboutToTrigger(RulePerformMomentumChange evt)
	{
		if (IsSuitable(evt))
		{
			Modifier.TryApply(evt.Modifiers, base.Fact, ModifierDescriptor);
		}
	}

	public void OnEventDidTrigger(RulePerformMomentumChange evt)
	{
	}

	private bool IsSuitable(RulePerformMomentumChange evt)
	{
		MomentumChangeReason changeReason = evt.ChangeReason;
		bool flag = changeReason == MomentumChangeReason.StartTurn || changeReason == MomentumChangeReason.KillEnemy;
		if (m_ChangeType == ChangeType.Positive)
		{
			if (!flag)
			{
				if (evt.ChangeReason == MomentumChangeReason.Custom)
				{
					return evt.RawValue > 0;
				}
				return false;
			}
			return true;
		}
		if (flag)
		{
			if (evt.ChangeReason == MomentumChangeReason.Custom)
			{
				return evt.RawValue < 0;
			}
			return false;
		}
		return true;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
