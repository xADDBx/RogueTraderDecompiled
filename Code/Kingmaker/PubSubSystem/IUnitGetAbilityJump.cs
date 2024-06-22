using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IUnitGetAbilityJump : ISubscriber
{
	void HandleUnitAbilityJumpDidActed(int distanceInCells);

	void HandleUnitResultJump(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster, bool useAttack);
}
