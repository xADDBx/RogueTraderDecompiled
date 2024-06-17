using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IClickActionHandler : ISubscriber
{
	void OnMoveRequested(Vector3 target);

	void OnCastRequested(AbilityData ability, TargetWrapper target);

	void OnItemUseRequested(AbilityData item, TargetWrapper target);

	void OnAbilityCastRefused(AbilityData ability, TargetWrapper target, IAbilityTargetRestriction failedRestriction);

	void OnAttackRequested(BaseUnitEntity unit, UnitEntityView target);
}
