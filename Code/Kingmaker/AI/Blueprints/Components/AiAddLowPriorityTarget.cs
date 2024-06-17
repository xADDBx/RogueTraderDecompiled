using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.AI.Blueprints.Components;

[AllowedOn(typeof(BlueprintBuff))]
[TypeId("4add24362e48459cb4d5e2ec9ff94dd4")]
public class AiAddLowPriorityTarget : EntityFactComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		base.Owner.GetOptional<PartUnitBrain>()?.AddCustomLowPriorityTarget(maybeCaster);
	}

	protected override void OnDeactivate()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		base.Owner.GetOptional<PartUnitBrain>()?.RemoveCustomLowPriorityTarget(maybeCaster);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
