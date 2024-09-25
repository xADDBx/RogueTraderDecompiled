using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.Exploration;

public abstract class AnomalyFactComponent<TComponent> : EntityFactComponent<AnomalyEntityData, TComponent>, IHashable where TComponent : BlueprintComponent
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
