using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class CloudEntity : StarSystemObjectEntity, IHashable
{
	public CloudEntity(string uniqueId, bool isInGame, BlueprintCloud blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public CloudEntity(StarSystemObjectView view, BlueprintCloud blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
	}

	protected CloudEntity(JsonConstructorMark _)
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
