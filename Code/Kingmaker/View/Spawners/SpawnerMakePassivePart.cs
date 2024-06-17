using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

public class SpawnerMakePassivePart : ViewBasedPart, IUnitInitializer, IHashable
{
	public void OnSpawn(AbstractUnitEntity unit)
	{
	}

	public void OnInitialize(AbstractUnitEntity unit)
	{
		unit.Passive.Retain();
	}

	public void OnDispose(AbstractUnitEntity unit)
	{
		unit.Passive.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
