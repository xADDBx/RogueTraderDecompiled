using Kingmaker.Mechanics.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

public abstract class AbstractUnitPart<TEntity> : MechanicEntityPart<TEntity>, IHashable where TEntity : AbstractUnitEntity
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
public abstract class AbstractUnitPart : AbstractUnitPart<AbstractUnitEntity>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
