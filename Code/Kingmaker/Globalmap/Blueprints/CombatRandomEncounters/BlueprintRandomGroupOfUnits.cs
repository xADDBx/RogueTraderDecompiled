using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Globalmap.CombatRandomEncounters;

namespace Kingmaker.Globalmap.Blueprints.CombatRandomEncounters;

[TypeId("20257e9e386b4d73a2b6fe68f51bab0e")]
public class BlueprintRandomGroupOfUnits : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintRandomGroupOfUnits>
	{
	}

	public UnitInGroupSettings[] Units;

	public int MinCount;

	public int MaxCount;
}
