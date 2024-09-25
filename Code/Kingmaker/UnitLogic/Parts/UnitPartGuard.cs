using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.View.Spawners;
using Newtonsoft.Json;
using StateHasher.Core;
using StateHasher.Core.Hashers;
using UnityEngine;

namespace Kingmaker.UnitLogic.Parts;

public class UnitPartGuard : BaseUnitPart, IHashable
{
	[JsonProperty]
	public bool UseLosInsteadOfVisibility;

	public readonly HashSet<BaseUnitEntity> DetectedUnits = new HashSet<BaseUnitEntity>();

	[JsonProperty]
	public float Range { get; private set; }

	[JsonProperty]
	public EntityRef<UnitSpawnerBase.MyData> Source { get; private set; }

	internal void Init(SpawnerGuardSettings.Part spawner)
	{
		Range = spawner.Source.Range;
		Source = new EntityRef<UnitSpawnerBase.MyData>(spawner.Owner.UniqueId);
		UseLosInsteadOfVisibility = spawner.Source.UseLosInsteadOfVisibility;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		float val2 = Range;
		result.Append(ref val2);
		EntityRef<UnitSpawnerBase.MyData> obj = Source;
		Hash128 val3 = StructHasher<EntityRef<UnitSpawnerBase.MyData>>.GetHash128(ref obj);
		result.Append(ref val3);
		result.Append(ref UseLosInsteadOfVisibility);
		return result;
	}
}
