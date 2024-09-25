using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("55a9ea4dc50c2e14ab72fa4526e74a84")]
public class WarhammerSpaceCombatEndTrigger : UnitFactComponentDelegate, IEndSpaceCombatHandler, ISubscriber, IHashable
{
	[SerializeField]
	public ActionList ActionsOnEnd;

	public void HandleEndSpaceCombat()
	{
		base.Fact.RunActionInContext(ActionsOnEnd, base.Owner.ToITargetWrapper());
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
