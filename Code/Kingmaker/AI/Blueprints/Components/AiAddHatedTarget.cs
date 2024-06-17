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
[TypeId("2823275d5b6e502419f0af44084652b4")]
public class AiAddHatedTarget : EntityFactComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		base.Owner.GetOptional<PartUnitBrain>()?.AddCustomHatedTarget(maybeCaster);
	}

	protected override void OnDeactivate()
	{
		MechanicEntity maybeCaster = base.Context.MaybeCaster;
		base.Owner.GetOptional<PartUnitBrain>()?.RemoveCustomHatedTarget(maybeCaster);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
