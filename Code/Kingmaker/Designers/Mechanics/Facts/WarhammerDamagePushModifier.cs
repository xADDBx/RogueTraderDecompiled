using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("1d3fcb8dd3cc46e3be7ec3330a41edef")]
public class WarhammerDamagePushModifier : UnitBuffComponentDelegate, IInitiatorRulebookHandler<RuleCalculateDamagePush>, IRulebookHandler<RuleCalculateDamagePush>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue StrengtBonushMultiplier = 1;

	public ContextValue StrengthBonusAddition = 0;

	public bool ForOneAttackAbilityType;

	[ShowIf("ForOneAttackAbilityType")]
	public AttackAbilityType Type;

	public void OnEventAboutToTrigger(RuleCalculateDamagePush evt)
	{
		if (!ForOneAttackAbilityType || evt.Reason.Ability.Blueprint.AttackType == Type)
		{
			evt.StrengthMultiplier = StrengtBonushMultiplier.Calculate(base.Context);
			evt.StrengthAddition = StrengthBonusAddition.Calculate(base.Context);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDamagePush evt)
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
