using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintEquipmentEnchantment))]
[TypeId("bc2d481c72bf4077b475e04491b6b97d")]
public class WarhammerRecoilModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public ContextValue RecoilBonusPercent;

	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon evt)
	{
		evt.RecoilModifiers.Add(ModifierType.PctAdd, RecoilBonusPercent.Calculate(base.Context), base.Fact);
	}

	public void OnEventDidTrigger(RuleCalculateStatsWeapon evt)
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
