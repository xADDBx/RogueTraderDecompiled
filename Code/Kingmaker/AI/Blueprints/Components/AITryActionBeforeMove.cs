using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Blueprints.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("b94597fc005d408082ef314616ca0d43")]
public class AITryActionBeforeMove : EntityFactComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		base.Owner.GetOptional<PartUnitBrain>()?.TryActionBeforeMove.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartUnitBrain>()?.TryActionBeforeMove.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
