using Kingmaker.EntitySystem.Entities;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.View.MapObjects.InteractionRestrictions;

public class ScriptRestrictionPart : InteractionRestrictionPart, IHashable
{
	public override int GetUserPriority(BaseUnitEntity user)
	{
		return -1;
	}

	public override bool CheckRestriction(BaseUnitEntity user)
	{
		return false;
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
