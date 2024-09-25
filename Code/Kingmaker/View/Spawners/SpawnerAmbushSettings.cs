using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("37e99300b8a24a429ee0076686fe7e50")]
public class SpawnerAmbushSettings : EntityPartComponent<SpawnerAmbushSettings.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerAmbushSettings Source => (SpawnerAmbushSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			PartUnitStealth optional = unit.GetOptional<PartUnitStealth>();
			if (optional != null)
			{
				optional.InAmbush = true;
				optional.AmbushJoinCombatDistance = Source.JoinCombatDistance;
				optional.AmbushTake20 = Source.StealthTake20;
				optional.ForceEnterStealth = true;
			}
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
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

	public float JoinCombatDistance = 5f;

	public bool StealthTake20;
}
