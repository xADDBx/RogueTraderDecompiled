using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[AllowMultipleComponents]
[TypeId("fa8dc9d6dc1bd454ea9210190aa0bd6b")]
public class TutorialTriggerUIEvent : TutorialTrigger, IUIEventHandler, ISubscriber, IHashable
{
	public UIEventType UIEvent;

	public void HandleUIEvent(UIEventType type)
	{
		if (type == UIEvent)
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
