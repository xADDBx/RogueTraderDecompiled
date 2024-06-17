using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

public class SpawnerOptimizedUnit : EntityPartComponent<SpawnerOptimizedUnit.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			unit.MarkExtra();
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

	[SerializeField]
	private bool m_IsLightweight;

	[JsonIgnore]
	public bool IsLightweight
	{
		get
		{
			if (m_IsLightweight)
			{
				return BlueprintRoot.Instance.UseLightweightUnit;
			}
			return false;
		}
	}
}
