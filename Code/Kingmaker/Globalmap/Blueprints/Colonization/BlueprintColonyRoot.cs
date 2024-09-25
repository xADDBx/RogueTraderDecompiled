using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.Globalmap.Blueprints.Colonization;

[TypeId("57e97e3d284c4783a511aed21a5c247c")]
public class BlueprintColonyRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintColonyRoot>
	{
	}

	public int MaxColoniesCount;

	public float ColonizationCostInPF = 1f;

	public int MinContentment;

	public int MaxContentment = 10;

	public int MinSecurity;

	public int MaxSecurity = 10;

	public int MinEfficiency;

	public int MaxEfficiency = 10;

	public int InitialMinerProductivity = 100;

	public int MinMinerProductivity;

	public int MaxMinerProductivity = 200;

	public List<BlueprintColonyReference> Colonies;

	public BlueprintColonyEventsRoot.Reference ColonyEvents;

	public List<BlueprintResourceReference> BasicResources;

	public List<BlueprintResourceReference> AllResources;
}
