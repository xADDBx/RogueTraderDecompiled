using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Starships;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c602a2cdfd853564cbc87ceef1e1e221")]
public class WarhammerStarshipHullHitTrigger : UnitFactComponentDelegate, IStarshipAttackHandler, ISubscriber, IHashable
{
	[SerializeField]
	public ActionList ActionsOnHullHit;

	[SerializeField]
	public bool ActivateIfInitiator;

	[SerializeField]
	public bool ActivateIfTarget;

	public void HandleAttack(RuleStarshipPerformAttack starshipAttack)
	{
		if ((!ActivateIfInitiator || base.Owner == starshipAttack.Initiator) && (!ActivateIfTarget || base.Owner == starshipAttack.TargetUnit))
		{
			_ = starshipAttack.ResultDamage;
			_ = 0;
		}
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
