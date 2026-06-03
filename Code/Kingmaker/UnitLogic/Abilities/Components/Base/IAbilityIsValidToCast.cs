using Kingmaker.EntitySystem.Entities;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityIsValidToCast
{
	bool IsValidToCast(TargetWrapper target, MechanicEntity caster, Vector3 casterPosition, out AbilityData.UnavailabilityReasonType reason);
}
