using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("2f26d7684d4d4650b745162edcb38dc7")]
public class TutorialTriggerDamagedAfterCombat : TutorialTrigger, IUnitCombatHandler, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	public void HandleUnitJoinCombat()
	{
	}

	public void HandleUnitLeaveCombat()
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit.Health.HitPointsLeft != unit.Health.MaxHitPoints)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = unit;
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
