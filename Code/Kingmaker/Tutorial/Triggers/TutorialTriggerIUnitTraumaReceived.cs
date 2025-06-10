using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("ce6032129297463facc9f76a42fc0aa1")]
public class TutorialTriggerIUnitTraumaReceived : TutorialTrigger, IUnitTraumaHandler, ISubscriber<IMechanicEntity>, ISubscriber, IHashable
{
	public void HandleTraumaReceived()
	{
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (baseUnitEntity != null && baseUnitEntity.IsInPlayerParty)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SourceUnit = EventInvokerExtensions.BaseUnitEntity;
			});
		}
	}

	public void HandleTraumaAvoided()
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
