using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("984f1f574d084e9ab538270e374c0768")]
public class TutorialTriggerExitToGlobalMap : TutorialTrigger, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>, ISubscriber, IHashable
{
	private int m_CountNeeded = 2;

	public void HandleStartScanningStarSystemObject()
	{
	}

	public void HandleScanStarSystemObject()
	{
		Game.Instance.Player.StarSystemsState.TutorialSsoCount++;
		if (Game.Instance.Player.StarSystemsState.TutorialSsoCount == m_CountNeeded + 1)
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
