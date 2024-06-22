using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View;

public class LocatorEntity : Entity, IHashable
{
	protected LocatorEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public LocatorEntity(LocatorView view)
		: base(view.UniqueId, view.IsInGameBySettings)
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
