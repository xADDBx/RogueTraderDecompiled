using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c96d8c2d5342c4f42848d3758aa21767")]
public class Untargetable : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.IsUntargetable.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.IsUntargetable.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
