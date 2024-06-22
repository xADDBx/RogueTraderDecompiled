using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.Globalmap.SystemMap;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[PlayerUpgraderAllowed(false)]
[TypeId("6e6e1011c5853b14f821d03cd8ee565d")]
public class ExplorePointOfInterest : GameAction
{
	public BlueprintPointOfInterestReference PointOfInterest;

	public override string GetCaption()
	{
		return "Set unique point of interest status to explored";
	}

	protected override void RunAction()
	{
		EntityPool<StarSystemObjectEntity> starSystemObjects = Game.Instance.State.StarSystemObjects;
		BasePointOfInterest basePointOfInterest = null;
		foreach (StarSystemObjectEntity item in starSystemObjects)
		{
			basePointOfInterest = item.PointOfInterests.FirstOrDefault((BasePointOfInterest poi) => poi.Blueprint == PointOfInterest?.Get());
			if (basePointOfInterest != null)
			{
				break;
			}
		}
		if (basePointOfInterest != null)
		{
			basePointOfInterest.SetInteracted();
			return;
		}
		List<BlueprintPointOfInterest> pointsExploredOutsideSystemMap = Game.Instance.Player.StarSystemsState.PointsExploredOutsideSystemMap;
		if (PointOfInterest != null && !pointsExploredOutsideSystemMap.Contains(PointOfInterest.Get()))
		{
			pointsExploredOutsideSystemMap.Add(PointOfInterest.Get());
		}
	}
}
