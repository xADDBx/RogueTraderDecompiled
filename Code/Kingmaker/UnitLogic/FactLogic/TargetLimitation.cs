using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Parts;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("73d94709d61e4014ab81bc25cb8d4c3b")]
public class TargetLimitation : UnitBuffComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Context.MaybeCaster?.GetOrCreate<UnitPartForbiddenAbilities>().AddEntry(base.Owner, base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Context.MaybeCaster?.GetOptional<UnitPartForbiddenAbilities>()?.RemoveEntry(base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
