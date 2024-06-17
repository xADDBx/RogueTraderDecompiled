using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.CombatRandomEncounters;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[TypeId("91a515db04514950af7f1ea65ea76ea9")]
public class BlueprintCombatRandomEncountersRoot : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintCombatRandomEncountersRoot>
	{
	}

	public List<CombatRandomEncounterSettings> Settings = new List<CombatRandomEncounterSettings>();
}
