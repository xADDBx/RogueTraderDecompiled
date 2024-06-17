using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("2f856626aa8b97b4189ffaeaf98561f1")]
public class TutorialTriggerUnitCondition : TutorialTrigger, IUnitConditionsChanged, ISubscriber<IAbstractUnitEntity>, ISubscriber, IHashable
{
	public UnitCondition TriggerCondition;

	public void HandleUnitConditionsChanged(UnitCondition condition)
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (condition == TriggerCondition && unit.Faction.IsPlayer && unit.State.HasCondition(condition))
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context[TutorialContextKey.SourceUnit] = unit;
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
