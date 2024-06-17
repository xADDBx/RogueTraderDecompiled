using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Enums;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.ActivatableAbilities.Restrictions;

[AllowMultipleComponents]
[ComponentName("AA restriction unit condition")]
[TypeId("8d32cf270a3f8df438c65ac197fca2d2")]
public class RestrictionHasUnitCondition : ActivatableAbilityRestriction, IHashable
{
	public UnitCondition Condition;

	public bool Invert;

	protected override bool IsAvailable()
	{
		if (!Invert)
		{
			return base.Owner.State.HasCondition(Condition);
		}
		return !base.Owner.State.HasCondition(Condition);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
