using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Starships;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.Utility;
using Pathfinding;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("847bad5ab8fbb6f4da7728b64b1245f8")]
public class StarshipModifyHitChancesWithDistance : UnitFactComponentDelegate, IInitiatorRulebookHandler<RuleStarshipCalculateHitChances>, IRulebookHandler<RuleStarshipCalculateHitChances>, ISubscriber, IInitiatorRulebookSubscriber, ITargetRulebookHandler<RuleStarshipCalculateHitChances>, ITargetRulebookSubscriber, IHashable
{
	private enum TriggerType
	{
		AsInitiator,
		AsTarget,
		Both
	}

	[SerializeField]
	private int hitPerTilePctBase;

	[SerializeField]
	private int hitPerTilePctPerTile;

	[SerializeField]
	[Tooltip("When calculating, ignore leading number of tiles")]
	private int tilesToIgnore;

	[SerializeField]
	private int tilesLimit;

	[SerializeField]
	private TriggerType triggerType;

	[SerializeField]
	private bool followDifficultySettings;

	public void OnEventAboutToTrigger(RuleStarshipCalculateHitChances evt)
	{
		if ((triggerType != 0 || evt.Initiator == base.Owner) && (triggerType != TriggerType.AsTarget || evt.Target == base.Owner))
		{
			int num = WarhammerGeometryUtils.DistanceToInCells(evt.Initiator.Position, default(IntRect), evt.Target.Position, default(IntRect));
			int num2 = ((num > tilesToIgnore) ? ((Math.Min(num, tilesLimit) - tilesToIgnore) * hitPerTilePctPerTile) : 0);
			int num3 = hitPerTilePctBase + num2;
			if (followDifficultySettings)
			{
				num3 = Mathf.RoundToInt((float)num3 * SpacecombatDifficultyHelper.StarshipAvoidanceMod(evt.Target));
			}
			evt.BonusHitChance += num3;
		}
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
