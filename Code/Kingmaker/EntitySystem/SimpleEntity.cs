using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.EntitySystem;

public abstract class SimpleEntity : Entity, IHashable
{
	protected SimpleEntity(IEntityViewBase view)
		: base(view.UniqueViewId, view.IsInGameBySettings)
	{
	}

	protected SimpleEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected SimpleEntity(JsonConstructorMark _)
		: base(_)
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
