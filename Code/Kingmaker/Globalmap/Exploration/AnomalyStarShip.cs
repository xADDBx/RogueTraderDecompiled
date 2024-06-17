using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Kingmaker.Utility.Attributes;

namespace Kingmaker.Globalmap.Exploration;

[AllowedOn(typeof(BlueprintAnomaly))]
[TypeId("26026b53f3de4e91b40ca335c5330336")]
public class AnomalyStarShip : BlueprintComponent
{
	public enum ShipTemper
	{
		Reckless = 1,
		Calculating,
		Coward
	}

	public ShipTemper Temper;

	public bool HasFaction;

	[ShowIf("HasFaction")]
	public FactionType Faction;

	public float Speed = 5f;
}
