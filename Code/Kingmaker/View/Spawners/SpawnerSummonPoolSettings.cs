using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.MapObjects;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("f831c87e967044318d77221783a731b1")]
public class SpawnerSummonPoolSettings : EntityPartComponent<SpawnerSummonPoolSettings.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerSummonPoolSettings Source => (SpawnerSummonPoolSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			BlueprintSummonPoolReference[] array = Source.Pools.EmptyIfNull();
			foreach (BlueprintSummonPoolReference blueprintSummonPoolReference in array)
			{
				if ((bool)blueprintSummonPoolReference.Get())
				{
					Game.Instance.SummonPools.Register(blueprintSummonPoolReference.Get(), unit);
				}
				else
				{
					PFLog.Default.Error(Source, Source.name + " nas broken summon pool listed");
				}
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

	public BlueprintSummonPoolReference[] Pools;
}
