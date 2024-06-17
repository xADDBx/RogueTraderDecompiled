using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c2eca07b264e7af46962536631163408")]
public class WarhammerOutgoingDodgeChancesBonus : UnitFactComponentDelegate, ITargetRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ISubscriber, ITargetRulebookSubscriber, IHashable
{
	public ContextValue Value;

	public bool OnlyAgainstCaster;

	public bool OnlyForAllies;

	public bool SpecificWeaponFamily;

	[ShowIf("SpecificWeaponFamily")]
	public WeaponFamily WeaponFamily = WeaponFamily.Bolt;

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		if ((!OnlyAgainstCaster || evt.MaybeAttacker == base.Context.MaybeCaster) && (!OnlyForAllies || evt.Defender.CombatGroup.IsAlly(evt.MaybeAttacker)) && (!SpecificWeaponFamily || evt.Ability?.Weapon?.Blueprint?.Family == WeaponFamily))
		{
			evt.DodgeValueModifiers.Add(Value.Calculate(base.Context), base.Fact);
		}
	}

	public void OnEventDidTrigger(RuleCalculateDodgeChance evt)
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
