using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic;
using StateHasher.Core;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Assets.Code.Designers.Mechanics.Facts;

[ComponentName("Modifes number of starship weapon maximum charges")]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("47c0913bfcaa42a4da671693b84ef65b")]
public class StarshipModifyMaxCharges : UnitFactComponentDelegate, IHashable
{
	[SerializeField]
	public StarshipWeaponType WeaponType;

	[SerializeField]
	public int Value;

	public override Hash128 GetHash128()
	{
		Hash128 result = default(Hash128);
		Hash128 val = base.GetHash128();
		result.Append(ref val);
		return result;
	}
}
