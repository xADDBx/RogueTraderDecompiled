using System;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Properties.Getters;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("36b9881761ae4529aa48aec231e3f060")]
public class ActionPointsGainedTrigger : ActionPointsChangedTrigger, IUnitGainActionPoints<EntitySubscriber>, IUnitGainActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitGainActionPoints, EntitySubscriber>, IUnitGainMovementPoints<EntitySubscriber>, IUnitGainMovementPoints, IEventTag<IUnitGainMovementPoints, EntitySubscriber>, IHashable
{
	public void HandleUnitGainActionPoints(int actionPoints, MechanicsContext context)
	{
		if (m_Type != 0 || !Restriction.IsPassed(base.Fact, context, null, null, base.Owner))
		{
			return;
		}
		using (ContextData<ActionPointsGainedGetter.ContextData>.Request().Setup(actionPoints))
		{
			base.Fact.RunActionInContext(Actions);
		}
	}

	public void HandleUnitGainMovementPoints(float movementPoints, MechanicsContext context)
	{
		if (m_Type != PointsType.Blue || !Restriction.IsPassed(base.Fact, context, null, null, base.Owner))
		{
			return;
		}
		using (ContextData<ActionPointsGainedGetter.ContextData>.Request().Setup((int)movementPoints))
		{
			base.Fact.RunActionInContext(Actions);
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
