using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Kingmaker.AreaLogic.TimeSurvival;

[Serializable]
[TypeId("8da007f57f974091916d05cbcc28116a")]
public class BlueprintUnitsList : BlueprintScriptableObject
{
	public List<BlueprintUnitReference> Units;

	public BlueprintUnitsList()
	{
		Units = new List<BlueprintUnitReference>();
	}

	public List<BlueprintUnit> GetBlueprintUnits()
	{
		return (from x in Units
			where x != null
			select x.Get()).ToList();
	}
}
