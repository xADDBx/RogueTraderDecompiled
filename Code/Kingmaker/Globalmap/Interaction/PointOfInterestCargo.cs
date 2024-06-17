using System;
using Kingmaker.Blueprints.Cargo;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.GameCommands;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestCargo : BasePointOfInterest, IHashable
{
	public new BlueprintPointOfInterestCargo Blueprint => (BlueprintPointOfInterestCargo)base.Blueprint;

	public PointOfInterestCargo(BlueprintPointOfInterestCargo blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestCargo(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		if (base.Status != ExplorationStatus.Explored)
		{
			Blueprint.ExplorationCargo.ForEach(delegate(BlueprintCargo c)
			{
				Game.Instance.GameCommandQueue.CreateCargoInternal(c);
			});
			ChangeStatusToInteracted();
			EventBus.RaiseEvent(delegate(IExplorationCargoHandler h)
			{
				h.HandlePointOfInterestCargoInteraction(this);
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
