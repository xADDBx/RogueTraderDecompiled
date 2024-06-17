using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;

namespace Warhammer.SpaceCombat.Blueprints;

[TypeId("63d735c21c194a84b3eb129a65af13f8")]
public class BlueprintShipSystemComponent : BlueprintScriptableObject
{
	[Serializable]
	public class Reference : BlueprintReference<BlueprintShipSystemComponent>
	{
	}

	public int[] UpgradeCost;

	public int HealthBonus;

	public int ProwRamDamageBonus;

	public int ProwRamSelfDamageReduceBonus;
}
