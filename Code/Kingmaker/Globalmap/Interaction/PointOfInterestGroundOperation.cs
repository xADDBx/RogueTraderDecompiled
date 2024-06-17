using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.DotNetExtensions;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Interaction;

[Serializable]
public class PointOfInterestGroundOperation : BasePointOfInterest, IHashable
{
	public new BlueprintPointOfInterestGroundOperation Blueprint => (BlueprintPointOfInterestGroundOperation)base.Blueprint;

	public PointOfInterestGroundOperation(BlueprintPointOfInterestGroundOperation blueprint)
		: base(blueprint)
	{
	}

	public PointOfInterestGroundOperation(JsonConstructorMark _)
		: base(_)
	{
	}

	public override void Interact(StarSystemObjectEntity entity)
	{
		if (base.Status == ExplorationStatus.Explored)
		{
			return;
		}
		base.Interact(entity);
		List<BlueprintUnit> requiredCompanions = Blueprint.RequiredCompanions;
		if (!requiredCompanions.Empty())
		{
			EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
			{
				h.HandleSetRequiredUnits(requiredCompanions);
			});
		}
		EventBus.RaiseEvent(delegate(IGroupChangerHandler h)
		{
			h.HandleCall(delegate
			{
				Game.Instance.LoadArea(Blueprint.AreaEnterPoint, Blueprint.AutoSaveMode);
			}, delegate
			{
			}, Game.Instance.LoadedAreaState.Settings.CapitalPartyMode);
		});
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
