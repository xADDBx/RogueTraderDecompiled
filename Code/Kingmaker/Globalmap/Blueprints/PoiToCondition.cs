using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Newtonsoft.Json;

namespace Kingmaker.Globalmap.Blueprints;

[Serializable]
public class PoiToCondition
{
	[JsonProperty]
	public ConditionsReference Conditions;

	[JsonProperty]
	public BlueprintPointOfInterestReference Poi;
}
