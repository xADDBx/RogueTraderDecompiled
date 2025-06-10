using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[TypeId("6443697e391c48aeac1474550f06cfc3")]
public class TutorialTriggerUnitReceivedPet : TutorialTrigger, IPetInitializedHandle, ISubscriber<IBaseUnitEntity>, ISubscriber, IHashable
{
	[SerializeField]
	private BlueprintUnitReference m_Unit;

	[SerializeField]
	private BlueprintPetReference m_Pet;

	public void HandlePetInitialized(BlueprintPet pet)
	{
		BaseUnitEntity petOwner = EventInvokerExtensions.BaseUnitEntity;
		if (petOwner != null && m_Unit.Get() == petOwner.Blueprint && m_Pet.Get() == pet)
		{
			TryToTrigger(null, delegate(TutorialContext context)
			{
				context.SolutionUnit = petOwner;
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
