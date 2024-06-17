using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityAoERadiusProvider
{
	int AoERadius { get; }

	TargetType Targets { get; }

	bool WouldTargetUnit(AbilityData ability, Vector3 targetPos, BaseUnitEntity unit);
}
