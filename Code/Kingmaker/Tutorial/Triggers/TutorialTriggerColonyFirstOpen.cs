using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("6182e4e67aee4600b7244142a0db6576")]
public class TutorialTriggerColonyFirstOpen : TutorialTrigger, IOpenExplorationScreenAfterColonization, ISubscriber, IHashable
{
	public void HandleTryOpenExplorationScreenAfterColonization(PlanetEntity planet)
	{
		TryToTrigger(null);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
