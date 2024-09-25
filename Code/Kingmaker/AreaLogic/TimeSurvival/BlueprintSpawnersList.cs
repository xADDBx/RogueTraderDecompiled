using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[TypeId("585edc54e62446a58844f30e5d2f1492")]
public class BlueprintSpawnersList : BlueprintScriptableObject
{
	public List<EntityReference> SpawnersPool;

	public BlueprintSpawnersList()
	{
		SpawnersPool = new List<EntityReference>();
	}
}
