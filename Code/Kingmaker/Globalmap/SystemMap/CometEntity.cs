using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class CometEntity : StarSystemObjectEntity, IHashable
{
	public CometEntity(string uniqueId, bool isInGame, BlueprintComet blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public CometEntity(StarSystemObjectView view, BlueprintComet blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
	}

	protected CometEntity(JsonConstructorMark _)
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
