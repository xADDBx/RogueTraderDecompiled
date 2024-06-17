using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("6ebdb034bf2f486b932f9342e175ec6d")]
public class AbilityLifecycleTriggerCaster : AbilityLifecycleTrigger, IAbilityExecutionProcessHandler<EntitySubscriber>, IAbilityExecutionProcessHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IAbilityExecutionProcessHandler, EntitySubscriber>, IHashable
{
	[SerializeField]
	private BlueprintMechanicEntityFact.Reference[] m_FactsToCheckOnOwner = new BlueprintMechanicEntityFact.Reference[0];

	public ReferenceArrayProxy<BlueprintMechanicEntityFact> FactsToCheckOnOwner
	{
		get
		{
			BlueprintReference<BlueprintMechanicEntityFact>[] factsToCheckOnOwner = m_FactsToCheckOnOwner;
			return factsToCheckOnOwner;
		}
	}

	public void HandleExecutionProcessStart(AbilityExecutionContext context)
	{
		if (FactsToCheckOnOwner.Any((BlueprintMechanicEntityFact p) => base.Owner.Facts.Contains(p)) || FactsToCheckOnOwner.Empty())
		{
			RunStartActions(context);
		}
	}

	public void HandleExecutionProcessEnd(AbilityExecutionContext context)
	{
		RunEndActions(context);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
