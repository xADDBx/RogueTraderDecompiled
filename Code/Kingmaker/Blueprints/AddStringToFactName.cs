using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("c8cf2b4638d44ca29565527b0d0b90c2")]
public class AddStringToFactName : AddStringToFact, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
