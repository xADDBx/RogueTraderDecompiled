using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IUnitJumpHandler : ISubscriber
{
	void HandleUnitJump(int distanceInCells, Vector3 startPoint, Vector3 targetPoint, MechanicEntity caster, BlueprintAbility ability);
}
