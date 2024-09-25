using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Blueprints;

[Serializable]
[AllowedOn(typeof(BlueprintUnit))]
[TypeId("0b7e8c6bec64d09469f436b66f2709de")]
public class CheckNewStatsComponent : EntityFactComponentDelegate, IHashable
{
	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
