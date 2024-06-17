using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class StarEntity : StarSystemObjectEntity, IHashable
{
	public StarEntity(string uniqueId, bool isInGame, BlueprintStar blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public StarEntity(StarSystemObjectView view, BlueprintStar blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
	}

	protected StarEntity(JsonConstructorMark _)
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
