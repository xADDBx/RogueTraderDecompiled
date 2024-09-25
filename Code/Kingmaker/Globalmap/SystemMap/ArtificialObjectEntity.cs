using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Globalmap.Blueprints.SystemMap;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Globalmap.SystemMap;

public class ArtificialObjectEntity : StarSystemObjectEntity, IHashable
{
	public ArtificialObjectEntity(string uniqueId, bool isInGame, BlueprintArtificialObject blueprint)
		: base(uniqueId, isInGame, blueprint)
	{
	}

	public ArtificialObjectEntity(StarSystemObjectView view, BlueprintArtificialObject blueprint)
		: base(view.UniqueId, view.IsInGameBySettings, blueprint)
	{
	}

	protected ArtificialObjectEntity(JsonConstructorMark _)
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
