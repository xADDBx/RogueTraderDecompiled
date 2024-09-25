using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic;

public abstract class UnitFactComponent<TComponent> : EntityFactComponent<BaseUnitEntity, TComponent>, IHashable where TComponent : BlueprintComponent
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
