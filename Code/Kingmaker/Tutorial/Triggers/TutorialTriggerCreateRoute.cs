using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SectorMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("126cc1c6f1a94e908d36474961ff84c4")]
public class TutorialTriggerCreateRoute : TutorialTrigger, ISectorMapScanHandler, ISubscriber<ISectorMapObjectEntity>, ISubscriber, IHashable
{
	private bool m_IsTriggered;

	public void HandleScanStarted(float range, float duration)
	{
	}

	public void HandleSectorMapObjectScanned(SectorMapPassageView passageToStarSystem)
	{
		if (EventInvokerExtensions.Entity is SectorMapObjectEntity && passageToStarSystem == null)
		{
			m_IsTriggered = true;
		}
	}

	public void HandleScanStopped()
	{
		if (m_IsTriggered)
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
