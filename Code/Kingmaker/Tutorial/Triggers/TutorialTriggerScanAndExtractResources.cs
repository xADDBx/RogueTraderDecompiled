using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("22c866e6440e45708662e014bf185615")]
public class TutorialTriggerScanAndExtractResources : TutorialTrigger, IScanStarSystemObjectHandler, ISubscriber<StarSystemObjectEntity>, ISubscriber, IHashable
{
	public void HandleStartScanningStarSystemObject()
	{
	}

	public void HandleScanStarSystemObject()
	{
		if (EventInvokerExtensions.Entity is StarSystemObjectEntity starSystemObjectEntity && !starSystemObjectEntity.Blueprint.Resources.EmptyIfNull().Empty())
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
