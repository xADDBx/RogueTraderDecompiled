using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.UnitLogic.Progression.Features;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Reduce damage received from scenario events")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("f7f3bbcfdfe6969469850bcc7ec00419")]
public class StarshipScenarioDamageBonus : BlueprintComponent
{
	[SerializeField]
	public int WarpDamagePct_Bonus;
}
