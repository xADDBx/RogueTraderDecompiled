using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("c3b35e6f79564a7dbce1629136b3ffd4")]
public class TutorialTriggerAPUsed : TutorialTrigger, IUnitActionPointsHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public void HandleRestoreActionPoints()
	{
	}

	public void HandleActionPointsSpent(BaseUnitEntity unit)
	{
		if (unit.IsPlayerFaction)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
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
