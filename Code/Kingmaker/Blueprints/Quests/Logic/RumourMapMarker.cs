using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Blueprints.Quests.Logic;

[AllowedOn(typeof(BlueprintQuestObjective))]
[TypeId("a8f17ebe8e49d634788d906b1f31602c")]
public class RumourMapMarker : BlueprintComponent
{
	[Tooltip("For rumours on Koronus map. Fill in either sector points or star system objects")]
	[SerializeField]
	public BlueprintSectorMapPointReference[] SectorMapPointsToVisit;

	[Tooltip("Image for rumours on Koronus")]
	[SerializeField]
	public Sprite SectorMapDestinationImage;

	[Tooltip("For rumours inside systems")]
	[SerializeField]
	private List<BlueprintStarSystemObjectReference> m_StarSystemObjectsToVisit;

	public List<BlueprintStarSystemObject> StarSystemObjectsToVisit => m_StarSystemObjectsToVisit?.EmptyIfNull().Dereference().ToList();
}
