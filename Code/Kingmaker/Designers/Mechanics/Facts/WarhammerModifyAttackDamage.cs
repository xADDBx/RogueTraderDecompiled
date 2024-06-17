using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("546c7961025e61b4fb1cad772c383a80")]
public class WarhammerModifyAttackDamage : MechanicEntityFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamage>, IRulebookHandler<RuleCalculateDamage>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	[SerializeField]
	private BlueprintBuffReference m_checkedBuff;

	public float multiplierPerRank;

	public BlueprintBuff CheckedBuff => m_checkedBuff.Get();

	public void OnEventAboutToTrigger(RuleCalculateDamage evt)
	{
		if (evt.Ability.Blueprint == base.OwnerBlueprint)
		{
			Buff buff = evt.MaybeTarget?.Buffs.GetBuff(CheckedBuff);
			if (buff != null)
			{
				int value = Mathf.RoundToInt((float)buff.Rank * multiplierPerRank * 100f);
				evt.ValueModifiers.Add(ModifierType.PctAdd, value, base.Fact);
			}
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamage evt)
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
