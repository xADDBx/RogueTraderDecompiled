using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core.Interfaces;
using UnityEngine;

namespace Kingmaker.PubSubSystem;

public interface IUnitGetAbilityPush : ISubscriber
{
	void HandleUnitResultPush(int distanceInCells, MechanicEntity caster, MechanicEntity target, Vector3 fromPoint);

	void HandleUnitAbilityPushDidActed(int distanceInCells);

	void HandleUnitResultPush(int distanceInCells, Vector3 targetPoint, MechanicEntity target, MechanicEntity caster);
}
