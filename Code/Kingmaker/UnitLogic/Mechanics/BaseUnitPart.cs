using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

public abstract class BaseUnitPart<TEntity> : MechanicEntityPart<TEntity>, IHashable where TEntity : BaseUnitEntity
{
	public override Type RequiredEntityType => EntityInterfacesHelper.BaseUnitEntityInterface;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
public abstract class BaseUnitPart : BaseUnitPart<BaseUnitEntity>, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
