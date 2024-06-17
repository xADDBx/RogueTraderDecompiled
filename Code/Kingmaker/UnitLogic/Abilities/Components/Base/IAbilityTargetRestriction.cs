using JetBrains.Annotations;
using Kingmaker.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.Base;

public interface IAbilityTargetRestriction
{
	bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition);

	[CanBeNull]
	string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition);
}
