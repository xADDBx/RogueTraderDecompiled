using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.View.Covers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.DodgeChance;

[TypeId("469780bd4d77420b8aa5ad36daca3561")]
public class WarhammerGlobalDodgeChanceModifier : WarhammerDodgeChanceModifier, IGlobalRulebookHandler<RuleCalculateDodgeChance>, IRulebookHandler<RuleCalculateDodgeChance>, ISubscriber, IGlobalRulebookSubscriber, IHashable
{
	public bool OnlyOnEnemies;

	public bool OnlyOnAllies;

	public bool OnlyOnTargetsInLineOfSight;

	public bool OnlyAgainstAllies;

	public void OnEventAboutToTrigger(RuleCalculateDodgeChance evt)
	{
		BaseUnitEntity initiatorUnit = evt.InitiatorUnit;
		if (initiatorUnit != null && (!OnlyOnAllies || initiatorUnit.IsAlly(base.Owner)) && (!OnlyOnEnemies || initiatorUnit.IsEnemy(base.Owner)) && (!OnlyOnTargetsInLineOfSight || (LosCalculations.CoverType)LosCalculations.GetWarhammerLos(base.Owner, initiatorUnit) != LosCalculations.CoverType.Invisible) && (!OnlyAgainstAllies || (evt.MaybeAttacker != null && evt.MaybeAttacker.IsAlly(evt.Defender) && evt.Defender.IsAlly(base.Owner))))
		{
			TryApply(evt);
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
