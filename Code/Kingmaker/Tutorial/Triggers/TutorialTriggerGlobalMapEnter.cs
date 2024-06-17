using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("e239b162c2df4ad1a9a82b744cd5c862")]
public class TutorialTriggerGlobalMapEnter : TutorialTrigger, ICloseLoadingScreenHandler, ISubscriber, IHashable
{
	public void HandleCloseLoadingScreen()
	{
		if (Game.Instance.CurrentlyLoadedArea.AreaStatGameMode == GameModeType.GlobalMap)
		{
			TryToTrigger(null);
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
