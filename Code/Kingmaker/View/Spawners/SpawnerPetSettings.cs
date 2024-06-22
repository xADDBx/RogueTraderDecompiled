using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Enums;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects;
using Owlcat.QA.Validation;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawnerBase))]
[KnowledgeDatabaseID("ba187735840a5ff4dbccadf936eeffed")]
public class SpawnerPetSettings : EntityPartComponent<SpawnerPetSettings.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerPetSettings Source => (SpawnerPetSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
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

	[ValidateNotNull]
	public Transform PetPoint;

	public PetType PetType;
}
