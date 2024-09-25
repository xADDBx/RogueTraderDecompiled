using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

namespace Kingmaker.Globalmap.CombatRandomEncounters;

[Serializable]
public class CombatRandomEncounterSettings
{
	public BlueprintAreaReference Area;

	public List<BlueprintAreaEnterPointReference> EnterPoints = new List<BlueprintAreaEnterPointReference>();

	public List<BlueprintRandomGroupOfUnits.Reference> UnitGroups = new List<BlueprintRandomGroupOfUnits.Reference>();
}
