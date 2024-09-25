using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Progression.Features;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[Obsolete]
[TypeId("965e937484aeea541a6b32e1d76d6e7f")]
public class SavesFixerRecalculate : UnitFactComponentDelegate, IHashable
{
	protected override void OnActivate()
	{
	}

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
