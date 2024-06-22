using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("f82d1f8ab69b4d3280fceff72436b2a4")]
public class SpawnerMimicSettings : EntityPartComponent<SpawnerMimicSettings.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
			UnitPartMimic orCreate = unit.GetOrCreate<UnitPartMimic>();
			if (!(((SpawnerMimicSettings)base.Source).AmbushObjectRef.FindData() is MapObjectEntity ambushObject))
			{
				PFLog.Default.Error("Invalid ambush object");
			}
			else
			{
				orCreate.AttachAmbushObject(ambushObject);
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

	[AllowedEntityType(typeof(MapObjectView))]
	public EntityReference AmbushObjectRef;
}
