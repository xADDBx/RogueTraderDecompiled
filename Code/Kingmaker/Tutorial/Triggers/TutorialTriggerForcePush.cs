using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("d16ec452277847dfb36bcab1bdb6794e")]
public class TutorialTriggerForcePush : TutorialTrigger, IUnitGetAbilityPush, ISubscriber, IHashable
{
	public void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint)
	{
		BaseUnitEntity unit = EventInvokerExtensions.BaseUnitEntity;
		if (unit != null && unit.IsPlayerFaction)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = unit;
			});
		}
	}

	public void HandleUnitAbilityPushDidActed(int distanceInCells)
	{
	}

	public void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster)
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
