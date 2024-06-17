using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View;

public class LocatorEntity : SimpleEntity, IHashable
{
	protected LocatorEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	public LocatorEntity(LocatorView view)
		: base(view.UniqueId, view.IsInGameBySettings)
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
