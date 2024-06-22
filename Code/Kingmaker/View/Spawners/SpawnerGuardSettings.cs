using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Mechanics.Entities;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.UnityExtensions;
using Kingmaker.View.MapObjects;
using Kingmaker.View.MapObjects.SriptZones;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.Spawners;

[RequireComponent(typeof(UnitSpawner))]
[DisallowMultipleComponent]
[KnowledgeDatabaseID("a5bdd1358d4c415cb7f6990e1abe6060")]
public class SpawnerGuardSettings : EntityPartComponent<SpawnerGuardSettings.Part>
{
	public class Part : ViewBasedPart, IUnitInitializer, IHashable
	{
		public new SpawnerGuardSettings Source => (SpawnerGuardSettings)base.Source;

		public void OnSpawn(AbstractUnitEntity unit)
		{
			unit.GetOrCreate<UnitPartGuard>().Init(this);
		}

		public void OnInitialize(AbstractUnitEntity unit)
		{
			if ((bool)Source.ExtendedVisionArea)
			{
				PartVision visionOptional = unit.GetVisionOptional();
				if (visionOptional != null)
				{
					visionOptional.ExtendedVisionArea = Source.ExtendedVisionArea;
				}
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

	public float Range = 5f;

	public int CoercionBonus;

	public ScriptZone ExtendedVisionArea;

	public bool UseLosInsteadOfVisibility;

	public ActionsReference OnDetect;

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		DebugDraw.DrawCircle(base.transform.position, Vector3.up, Range);
	}
}
