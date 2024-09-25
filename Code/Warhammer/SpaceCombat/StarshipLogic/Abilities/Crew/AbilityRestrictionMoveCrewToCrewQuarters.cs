using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[AllowedOn(typeof(BlueprintAbility))]
[TypeId("83e58ba87dbb49608c53011182f96c53")]
public class AbilityRestrictionMoveCrewToCrewQuarters : BlueprintComponent, IAbilityRestriction
{
	[SerializeField]
	private ShipModuleType m_ModuleType;

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		if (!(ability?.Fact?.Owner is StarshipEntity starshipEntity))
		{
			return false;
		}
		if (starshipEntity.Crew.GetReadOnlyCrewData(ShipModuleType.CrewQuarters).CanMoveTo())
		{
			return starshipEntity.Crew.GetReadOnlyCrewData(m_ModuleType).CanMoveFrom();
		}
		return false;
	}

	public string GetAbilityRestrictionUIText()
	{
		return "<Crew quarters must have empty space>";
	}
}
