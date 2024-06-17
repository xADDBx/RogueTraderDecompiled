using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Timeline;

public class DirectorAdapterEntity : SimpleEntity, IHashable
{
	public DirectorAdapterEntity(EntityViewBase view)
		: base(view)
	{
	}

	public DirectorAdapterEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected DirectorAdapterEntity(JsonConstructorMark _)
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
