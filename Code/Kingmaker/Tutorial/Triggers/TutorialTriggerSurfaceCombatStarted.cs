using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("edba194b5f694bfcb57475c59345dce5")]
public class TutorialTriggerSurfaceCombatStarted : TutorialTrigger, ITurnBasedModeHandler, ISubscriber, IHashable
{
	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = GameHelper.GetPlayerCharacter();
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
