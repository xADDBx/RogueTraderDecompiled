using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem.Entities;

public class MechanicLightRoot : MechanicEntity, IHashable
{
	public override bool IsAffectedByFogOfWar => false;

	public MechanicLightRoot(JsonConstructorMark _)
		: base(_)
	{
	}

	public MechanicLightRoot(string uniqueId, bool isInGame, BlueprintMechanicEntityFact blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	protected override IEntityViewBase CreateViewForData()
	{
		return null;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
