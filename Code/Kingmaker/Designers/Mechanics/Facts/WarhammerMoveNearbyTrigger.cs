using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4a7ab8abbb664d7584a4317cb8e5da41")]
public class WarhammerMoveNearbyTrigger : UnitFactComponentDelegate, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public ActionList ActionOnSelf;

	public ActionList ActionsOnUnitNearby;

	[SerializeField]
	private BlueprintBuffReference m_CooldownBuff;

	public BlueprintBuff CooldownBuff => m_CooldownBuff?.Get();

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (base.Owner.DistanceToInCells(unit) <= 1 && !unit.Facts.Contains(CooldownBuff))
		{
			base.Fact.RunActionInContext(ActionOnSelf, base.OwnerTargetWrapper);
			base.Fact.RunActionInContext(ActionsOnUnitNearby, unit.ToITargetWrapper());
			unit.Buffs.Add(CooldownBuff, base.Context);
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
