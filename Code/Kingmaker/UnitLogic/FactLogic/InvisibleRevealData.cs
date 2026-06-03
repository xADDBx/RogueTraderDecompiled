using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

public readonly struct InvisibleRevealData : IHashable
{
	public readonly EntityRef EntityRef;

	public readonly bool InterruptPlayerMovement;

	public InvisibleRevealData(IEntity entity, bool interruptPlayerMovement)
	{
		EntityRef = new EntityRef(entity);
		InterruptPlayerMovement = interruptPlayerMovement;
	}

	public Hash128 GetHash128()
	{
		return default(Hash128);
	}
}
