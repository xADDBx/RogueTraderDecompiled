using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("183a8309a92341a0b44b0a0b2363c8a9")]
public class TutorialTriggerCantFullyRepairShip : TutorialTrigger, IRepairShipHandler, ISubscriber, IHashable
{
	public void HandleCantFullyRepairShip()
	{
		PartStarshipHull hull = Game.Instance.Player.PlayerShip.Hull;
		if (hull.ProwRam.UpgradeLevel > 1 || hull.InternalStructure.UpgradeLevel > 1)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = Game.Instance.Player.PlayerShip;
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
