using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("0b32d441ae614378928e5bab8eec92e8")]
public class WarhammerPriorityTarget : UnitBuffComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
		(base.Context?.MaybeCaster)?.GetOrCreate<UnitPartPriorityTarget>().AddTarget(base.Buff);
	}

	protected override void OnDeactivate()
	{
		(base.Context?.MaybeCaster)?.GetOptional<UnitPartPriorityTarget>()?.RemoveTarget(base.Buff);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
