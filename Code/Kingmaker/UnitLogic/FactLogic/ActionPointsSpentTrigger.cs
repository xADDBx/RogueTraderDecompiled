using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("e36bc5ac422a452cb2757717cbd3f5a4")]
public class ActionPointsSpentTrigger : ActionPointsChangedTrigger, IUnitSpentActionPoints<EntitySubscriber>, IUnitSpentActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitSpentActionPoints, EntitySubscriber>, IUnitSpentMovementPoints<EntitySubscriber>, IUnitSpentMovementPoints, IEventTag<IUnitSpentMovementPoints, EntitySubscriber>, IHashable
{
	[SerializeField]
	private ContextValue m_TriggerValue;

	private int TriggerValue => m_TriggerValue.Calculate(base.Context);

	public void HandleUnitSpentActionPoints(int actionPointsSpent)
	{
		if (m_Type == PointsType.Yellow || Restriction.IsPassed(base.Fact, base.Owner))
		{
			int actionPointsYellow = base.Owner.CombatState.ActionPointsYellow;
			if (actionPointsYellow <= TriggerValue && (actionPointsYellow + actionPointsSpent > TriggerValue || actionPointsSpent == -1))
			{
				base.Fact.RunActionInContext(Actions);
			}
		}
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		if (m_Type == PointsType.Blue || Restriction.IsPassed(base.Fact, base.Owner))
		{
			float actionPointsBlue = base.Owner.CombatState.ActionPointsBlue;
			if (actionPointsBlue <= (float)TriggerValue && (actionPointsBlue + movementPointsSpent > (float)TriggerValue || movementPointsSpent < 0f))
			{
				base.Fact.RunActionInContext(Actions);
			}
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
