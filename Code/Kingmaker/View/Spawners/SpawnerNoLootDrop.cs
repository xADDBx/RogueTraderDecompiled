using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("b388b496d46f4b1887fc7ba2996dae11")]
public class SpawnerNoLootDrop : EntityPartComponent<SpawnerNoLootDrop.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
			unit.Parts.GetOrCreate<UnitPartUnlootable>();
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
		}

		public void OnDispose(AbstractUnitEntity unit)
		{
			unit.Parts.Remove<UnitPartUnlootable>();
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
