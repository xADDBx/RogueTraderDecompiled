using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("2f54dfbdcdce4761bcfd711b2409c34b")]
public class AddStringToFactDescription : AddStringToFact, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
