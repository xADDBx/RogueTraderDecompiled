using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Visual.Sound;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("41b13ee3f268d3e418f925e547ed6543")]
public class TutorialTriggerShipMoved : TutorialTrigger, IUnitMoveHandler, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	private bool m_IsTriggered;

	public void HandleUnitMovement(AbstractUnitEntity unit)
	{
		if (!unit.IsInCombat || !unit.IsPlayerShip() || m_IsTriggered)
		{
			return;
		}
		StarshipEntity ship = unit as StarshipEntity;
		if (ship != null)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = ship;
			});
			m_IsTriggered = true;
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
