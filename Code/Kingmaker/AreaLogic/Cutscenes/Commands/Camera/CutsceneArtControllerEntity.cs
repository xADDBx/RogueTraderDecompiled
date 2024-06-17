using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands.Camera;

public class CutsceneArtControllerEntity : SimpleEntity, IHashable
{
	public CutsceneArtControllerEntity(EntityViewBase view)
		: base(view)
	{
	}

	public CutsceneArtControllerEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected CutsceneArtControllerEntity(JsonConstructorMark _)
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
