using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add weapons for show")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4cc38c8a3e3ea5d44afe498c6fa228c3")]
public class AddCalculatedWeapon : UnitFactComponentDelegate, IHashable
{
	public CalculatedWeapon Weapon;

	public bool ScaleDamageByRank;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
