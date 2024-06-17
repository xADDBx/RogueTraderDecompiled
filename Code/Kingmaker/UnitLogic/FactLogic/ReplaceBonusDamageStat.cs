using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[ComponentName("Add condition")]
[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[AllowMultipleComponents]
[TypeId("960638d10abe4b71a23cad4873b9a5d5")]
public class ReplaceBonusDamageStat : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleCalculateStatsWeapon>, IRulebookHandler<RuleCalculateStatsWeapon>, ISubscriber, IInitiatorRulebookSubscriber, IHashable
{
	public StatType NewStat;

	public bool ReplaceStrengthForMeleeDamage;

	public bool ReplaceIntelligenceForRangedAreaDamage;

	public void OnEventAboutToTrigger(RuleCalculateStatsWeapon evt)
	{
		if (ReplaceStrengthForMeleeDamage)
		{
			evt.MeleeDamageStats.Add(NewStat);
		}
		if (ReplaceIntelligenceForRangedAreaDamage)
		{
			evt.RangedAreaDamageStats.Add(NewStat);
		}
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
