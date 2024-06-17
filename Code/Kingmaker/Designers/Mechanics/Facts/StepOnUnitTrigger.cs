using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("78416892d808478298265d953095c2ae")]
public class StepOnUnitTrigger : UnitFactComponentDelegate, IUnitMovementHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList MoveThroughUnitActions;

	public void HandleWaypointUpdate(int index)
	{
		BaseUnitEntity baseUnitEntity = ContextData<EventInvoker>.Current?.InvokerEntity as BaseUnitEntity;
		if (base.Fact.Owner != baseUnitEntity)
		{
			return;
		}
		foreach (CustomGridNodeBase occupiedNode in baseUnitEntity.GetOccupiedNodes())
		{
			if (!occupiedNode.TryGetUnit(out var unit) || baseUnitEntity == unit)
			{
				continue;
			}
			using (base.Context.GetDataScope(unit.ToITargetWrapper()))
			{
				if (Restrictions.IsPassed(base.Fact, base.Owner))
				{
					MoveThroughUnitActions?.Run();
				}
			}
		}
	}

	public void HandleMovementComplete()
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
