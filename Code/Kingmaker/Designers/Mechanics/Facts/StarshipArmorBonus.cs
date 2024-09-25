using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Add Starship armor bonus")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("4e6259867a7ae7447b5c8336ba1bb332")]
public class StarshipArmorBonus : BlueprintComponent
{
	[SerializeField]
	public int ArmourFore;

	[SerializeField]
	public int ArmourPort;

	[SerializeField]
	public int ArmourStarboard;

	[SerializeField]
	public int ArmourAft;
}
