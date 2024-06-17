using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public class SoulMark : Feature, IHashable
{
	public SoulMark(BlueprintFeature blueprint, BaseUnitEntity owner, MechanicsContext parentContext = null)
		: base(blueprint, owner, parentContext)
	{
	}

	public SoulMark(JsonConstructorMark _)
		: base(_)
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
