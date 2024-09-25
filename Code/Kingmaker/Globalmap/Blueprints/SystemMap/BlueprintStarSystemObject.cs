using System.Collections.Generic;
using System.Linq;
using Kingmaker.BarkBanters;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Area;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.Blueprints.Colonization;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Globalmap.Interaction;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.Globalmap.Blueprints.SystemMap;

[TypeId("13ca9818e9a94731b585c126f7e8e92f")]
public class BlueprintStarSystemObject : BlueprintMechanicEntityFact
{
	public bool IsQuestObject;

	[SerializeField]
	public bool IsScannedOnStart;

	[SerializeField]
	public ResourceData[] Resources;

	[SerializeField]
	public BlueprintBarkBanterList.Reference BarkBanterList;

	[SerializeField]
	private List<BlueprintAreaReference> m_ConnectedAreas;

	public virtual bool ShouldBeHighlighted => false;

	public List<BlueprintArea> ConnectedAreas => m_ConnectedAreas.EmptyIfNull().Dereference().ToList();

	public void ResaveConnectedAreas()
	{
		IEnumerable<PointOfInterestGroundOperationComponent> components = this.GetComponents<PointOfInterestGroundOperationComponent>();
		if (m_ConnectedAreas == null)
		{
			m_ConnectedAreas = new List<BlueprintAreaReference>();
		}
		else
		{
			m_ConnectedAreas.Clear();
		}
		foreach (PointOfInterestGroundOperationComponent item in components)
		{
			BlueprintPointOfInterestGroundOperation blueprintPointOfInterestGroundOperation = item.PointBlueprint as BlueprintPointOfInterestGroundOperation;
			if (blueprintPointOfInterestGroundOperation?.AreaEnterPoint.Area != null)
			{
				m_ConnectedAreas.Add(blueprintPointOfInterestGroundOperation.AreaEnterPoint.Area.ToReference<BlueprintAreaReference>());
			}
			if (blueprintPointOfInterestGroundOperation?.AdditionalArea != null)
			{
				m_ConnectedAreas.Add(blueprintPointOfInterestGroundOperation.AdditionalArea.ToReference<BlueprintAreaReference>());
			}
		}
	}
}
