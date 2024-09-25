using System;
using System.Collections.Generic;
using Kingmaker.AI;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.Attributes;

namespace Warhammer.SpaceCombat.AI;

[Serializable]
public class AbilitySettings
{
	public enum AbilityCastSpotType
	{
		Random,
		Earlier,
		Later,
		CloserToTarget,
		CloserToOptimum
	}

	public AbilitySourceWrapper AbilitySource;

	public PropertyCalculator AbilityValue;

	public AbilityCastSpotType AbilityCastSpot;

	[ShowIf("IsCloserToOptimumCastSpotType")]
	public int OptimumDistance;

	public List<BlueprintAbility> Abilities => AbilitySource?.Abilities;

	private bool IsCloserToOptimumCastSpotType => AbilityCastSpot == AbilityCastSpotType.CloserToOptimum;
}
