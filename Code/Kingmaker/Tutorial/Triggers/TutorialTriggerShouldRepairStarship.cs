using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("b3108c602f5f4824ac36f3f61cdb85c8")]
public class TutorialTriggerShouldRepairStarship : TutorialTrigger, IEndSpaceCombatHandler, ISubscriber, IHashable
{
	public void HandleEndSpaceCombat()
	{
		if (Game.Instance.Player.PlayerShip.Health.Damage > 0)
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
