using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Colonization;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("513a835d99ab4783a78c6691374dbb64")]
public class TutorialTriggerColonyEvent : TutorialTrigger, IColonizationEventHandler, ISubscriber, IHashable
{
	public void HandleEventStarted(Colony colony, BlueprintColonyEvent colonyEvent)
	{
		TryToTrigger(null);
	}

	public void HandleEventFinished(Colony colony, BlueprintColonyEvent colonyEvent, BlueprintColonyEventResult result)
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
