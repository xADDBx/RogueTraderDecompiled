using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class AsteroidEntity : StarSystemObjectEntity, IHashable
{
	public AsteroidEntity(string uniqueId, bool isInGame, BlueprintAsteroid blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public AsteroidEntity(StarSystemObjectView view, BlueprintAsteroid blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
	}

	protected AsteroidEntity(JsonConstructorMark _)
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
