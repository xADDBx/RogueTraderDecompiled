using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Increase by 1 attacks count in burst for choosen weapon group")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("7cf44657d29b8e548b28bfc8f2f26fdc")]
public class StarshipBurstAttackCountIncrease : BlueprintComponent
{
	[SerializeField]
	public StarshipWeaponType weaponType;

	[SerializeField]
	public int Chances;
}
