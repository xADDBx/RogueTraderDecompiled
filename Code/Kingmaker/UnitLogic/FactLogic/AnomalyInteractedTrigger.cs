using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Globalmap.Exploration;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("b658011e006a10048a06181cd02105e3")]
public class AnomalyInteractedTrigger : UnitFactComponentDelegate, IAnomalyHandler, ISubscriber<AnomalyEntityData>, ISubscriber, IHashable
{
	public ActionList Action;

	public void HandleAnomalyInteracted()
	{
		base.Fact.RunActionInContext(Action, base.Context.MaybeOwner.ToITargetWrapper());
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
