using System;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestResources : BasePointOfInterest, IHashable
{
	public new BlueprintPointOfInterestResources Blueprint => (BlueprintPointOfInterestResources)base.Blueprint;

	public PointOfInterestResources(BlueprintPointOfInterestResources blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestResources(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		if (base.Status != ExplorationStatus.Explored)
		{
			base.Interact(entity);
			entity.AddResource(Blueprint.ExplorationResource.Resource.Get(), Blueprint.ExplorationResource.Count);
			ChangeStatusToInteracted();
		}
	}

	public override bool IsVisible()
	{
		if (base.Status != ExplorationStatus.Explored)
		{
			return base.IsVisible();
		}
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
