using Kingmaker.Controllers.Optimization;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.View;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes;

public class CutsceneAnchorEntity : SimpleEntity, IHashable
{
	public override bool IsSuppressible => true;

	public override bool IsAffectedByFogOfWar => true;

	public CutsceneAnchorEntity(EntityViewBase view)
		: base(view)
	{
	}

	public CutsceneAnchorEntity(string uniqueId, bool isInGame)
		: base(uniqueId, isInGame)
	{
	}

	protected CutsceneAnchorEntity(JsonConstructorMark _)
		: base(_)
	{
	}

	protected override void OnCreateParts()
	{
		base.OnCreateParts();
		GetOrCreate<EntityBoundsPart>();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
