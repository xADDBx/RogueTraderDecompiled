using System;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics;

public abstract class MechanicEntityPart<TEntity> : EntityPart<TEntity>, IHashable where TEntity : MechanicEntity
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
public abstract class MechanicEntityPart : MechanicEntityPart<MechanicEntity>, IHashable
{
	public override Type RequiredEntityType => EntityInterfacesHelper.MechanicEntityInterface;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
