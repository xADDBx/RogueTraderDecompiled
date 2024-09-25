using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c9c11482ab60d5544b32f549619f6818")]
public class WarhammerDodgeModifier : UnitFactComponentDelegate, IHashable
{
	public float dodgeModifer = 1f;

	[Range(0f, 1f)]
	public float dodgeMinimumEnsured;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
