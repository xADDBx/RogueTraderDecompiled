using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[TypeId("69961325deb248f78eac3cdc2fba755e")]
public class CasterExtraTurnBonus : UnitFactComponentDelegate, IHashable
{
	public ContextValue ActionPointsBonus;

	public ContextValue MovementPointsBonus;

	public ActionList ActionsOnTarget;

	public bool OnlyIfTargetIsNotOwner = true;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
