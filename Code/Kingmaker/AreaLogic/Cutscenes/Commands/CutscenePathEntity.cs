using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

public class CutscenePathEntity : SimpleEntity, IHashable
{
	public CutscenePathEntity(EntityViewBase view)
		: base(view)
	{
	}

	public CutscenePathEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected CutscenePathEntity(JsonConstructorMark _)
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
