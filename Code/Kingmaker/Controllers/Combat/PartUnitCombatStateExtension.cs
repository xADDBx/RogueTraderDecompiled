using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using UnityEngine;

namespace Kingmaker.Controllers.Combat;

public static class PartUnitCombatStateExtension
{
	[CanBeNull]
	public static PartUnitCombatState GetCombatStateOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitCombatState>();
	}

	public static bool IsEngagedInMelee(this MechanicEntity entity)
	{
		return entity.GetCombatStateOptional()?.IsEngaged ?? false;
	}

	[CanBeNull]
	public static Vector3? GetLastAttackPosition(this MechanicEntity entity)
	{
		return entity.GetCombatStateOptional()?.LastAttackPosition;
	}
}
