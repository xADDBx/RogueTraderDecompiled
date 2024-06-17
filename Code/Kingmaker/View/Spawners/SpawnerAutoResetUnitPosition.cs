using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawnerBase))]
[DisallowMultipleComponent]
public class SpawnerAutoResetUnitPosition : EntityPartComponent<SpawnerAutoResetUnitPosition.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			unit.SetPositionWithoutWaking(base.Owner.Position);
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
		}

		public override Hash128 GetHash128()
		{
			Hash128 result = default(Hash128);
			Hash128 val = base.GetHash128();
			result.Append(ref val);
			return result;
		}
	}
}
