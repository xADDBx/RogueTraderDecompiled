using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Serializable]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("a2a3cedafc6d40968174875aecc22b54")]
public class DotImmunityBuff : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.GetOrCreate<DOTLogic.PartDOTDirector>()?.RetainSkipEffect(base.Fact);
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<DOTLogic.PartDOTDirector>()?.ReleaseSkipEffect(base.Fact);
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
