using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.View.MapObjects;
using Newtonsoft.Json;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[KnowledgeDatabaseID("9496b0e052b844e4a7587da8d55f17db")]
public class SpawnerOptimizedUnit : EntityPartComponent<SpawnerOptimizedUnit.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public void OnSpawn(AbstractUnitEntity unit)
		{
			SpawnerOptimizedUnit spawnerOptimizedUnit = (SpawnerOptimizedUnit)base.Source;
			unit.FreezeOutsideCamera = spawnerOptimizedUnit.m_FreezeOutsideCamera;
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			if (((SpawnerOptimizedUnit)base.Source).m_IsExtra)
			{
				unit.MarkExtra();
			}
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
	[Tooltip("If set, unit will be deactivate outside camera and cutscene associated with it will be set on pause (true pause)")]
	private bool m_FreezeOutsideCamera;

	[SerializeField]
	private bool m_IsExtra = true;

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
