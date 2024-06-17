using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Assets.Code.Designers.Mechanics.Facts.DodgeChance;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bb50529527787d544b395cdbed76eda2")]
public class StarshipEvasionModifier : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleStarshipCalculateHitChances>, IRulebookHandler<RuleStarshipCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleStarshipCalculateHitChances>, ITargetRulebookSubscriber, IHashable
{
	private enum ModifyWhen
	{
		IsInitiator,
		IsTarget
	}

	[SerializeField]
	private ModifyWhen modifyWhen = ModifyWhen.IsTarget;

	public int EvasionBonusPct;

	[SerializeField]
	private BlueprintFeatureReference m_CheckEnemyFeature;

	public BlueprintFeature CheckEnemyFeature => m_CheckEnemyFeature?.Get();

	public void OnEventAboutToTrigger(RuleStarshipCalculateHitChances evt)
	{
		if ((modifyWhen != 0 || (evt.Initiator == base.Owner && CheckF(evt.Target))) && (modifyWhen != ModifyWhen.IsTarget || (evt.Target == base.Owner && CheckF(evt.Initiator))))
		{
			evt.BonusEvasionChance += EvasionBonusPct;
		}
	}

	private bool CheckF(StarshipEntity enemy)
	{
		if (CheckEnemyFeature != null)
		{
			return enemy.Facts.Contains(CheckEnemyFeature);
		}
		return true;
	}

	public void OnEventDidTrigger(RuleStarshipCalculateHitChances evt)
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
