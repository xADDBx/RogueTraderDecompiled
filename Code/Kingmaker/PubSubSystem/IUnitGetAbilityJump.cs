using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IUnitGetAbilityJump : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleUnitAbilityJumpDidActed(int distanceInCells);

	void HandleUnitResultJump(int distanceInCells, Vector3 targetPoint, bool directJump, MechanicEntity target, MechanicEntity caster, bool useAttack);
}
public interface IUnitGetAbilityJump<TTag> : IUnitGetAbilityJump, ISubscriber<IBaseUnitEntity>, ISubscriber, IEventTag<IUnitGetAbilityJump, TTag>
{
}
