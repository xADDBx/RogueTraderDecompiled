using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("22f8af664e0f421b81baa98cd36b8b69")]
public class ForbidRotation : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Features.RotationForbidden.Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Features.RotationForbidden.Release();
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
